import math
import socket

# 서버의 주소입니다. hostname 또는 ip address를 사용할 수 있습니다.
import time

def DecodeServer_Protocool(values):
    print(values)

    data = values.split(",")
    if data[0] =='<PTP>':
        datass = values.strip(";")
        datas = datass+"," + round(time.time() * 1000).__str__()+";"
        client_socket.sendall(datas.encode())
    elif data[0] =='#4':
        print("Protocool : #4 = AI Start")
    elif data[0] =='#5':
        print("Protocool : #5 = AI Data Send")
        datas = "AI FINISH"
        client_socket.sendall(datas.encode())
    elif data[0]=='END;':
        ## 예제문
        client_socket.sendall('#5;'.encode())
    elif data[0]=='#11':
        print("DSE")


HOST = '127.0.0.1'
# 서버에서 지정해 놓은 포트 번호입니다.
PORT = 4545

# 소켓 객체를 생성합니다.
# 주소 체계(address family)로 IPv4, 소켓 타입으로 TCP 사용합니다.
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# 지정한 HOST와 PORT를 사용하여 서버에 접속합니다.
client_socket.connect((HOST, PORT))
client_socket.sendall('%^&AI;'.encode())
# 메시지를 전송합니다.

# 메시지를 수신합니다.

while True:
    data = client_socket.recv(1024)
    print('Received', repr(data.decode()))
    DecodeServer_Protocool(data.decode())

#
#
# # 소켓을 닫습니다.
# client_socket.close()