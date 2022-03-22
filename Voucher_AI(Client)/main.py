import socket
import time

host = '127.0.0.1'
port = 4545

def current_milli_time():
    return round(time.time() * 1000)


client_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)

client_socket.connect((host,port))
client_socket.send('%^&AI;'.encode('utf-8'))

while 1:
    data = client_socket.recv(1024)

    received_data = data.decode('utf-8')
    times = str(current_milli_time())
    splited_datas = received_data.split(';')

    for i in range(len(splited_datas)-1):
        if splited_datas[i] == '':
            splited_datas.remove(splited_datas[i])
        else:
            pass
        splited_data=splited_datas[i].split(',')
        for j in range(len(splited_data)):
            if splited_data[j]=="<PTP>":
                senddata = splited_datas[i]+","+times+";"
                client_socket.send(senddata.encode('utf-8'))

            if splited_data[j] == "#4":
                pass






