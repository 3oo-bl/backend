import os

import grpc
from concurrent import futures
from searchers.wb_searcher import WBParserService
from searchers.ozon_parser import OzonParserService
import searchers_pb2_grpc

def serve():
    with open("/run/secrets/server_key", "rb") as f:
        private_key = f.read()

    with open("/run/secrets/server_crt", "rb") as f:
        certificate = f.read()
        
    creds = grpc.ssl_server_credentials(
        [(private_key, certificate)],
        root_certificates=None,
        require_client_auth=False
    )

    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))

    searchers_pb2_grpc.add_WbParserServicer_to_server(WBParserService(), server)
    searchers_pb2_grpc.add_OzonParserServicer_to_server(OzonParserService(), server)
    server.add_secure_port('0.0.0.0:50051', creds)
    print("Server started on port 50051")
    server.start()
    server.wait_for_termination()

if __name__ == '__main__':
    serve()