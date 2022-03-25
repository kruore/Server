import socket
import time

host = '127.0.0.1'
port = 4545

def current_milli_time():
    return round(time.time() * 1000)

isActive = False
client_socket = socket.socket(socket.AF_INET,socket.SOCK_STREAM)

client_socket.connect((host,port))
client_socket.send('%^&AI;'.encode('utf-8'))

while 1:
    data = client_socket.recv(1024)

    received_data = data.decode('utf-8')
    times = str(current_milli_time())
    print(received_data)
    splited_datas = received_data.split(';')

    for i in range(len(splited_datas)-1):
        if splited_datas[i] == '':
            splited_datas.remove(splited_datas[i])
        else:
            pass
        splited_data=splited_datas[i].split(',')
        for j in range(len(splited_data)):

            if splited_data[j] is None:
                pass

            elif splited_data[j]=="<PTP>":
                senddata = splited_datas[i]+","+times+";"
                client_socket.send(senddata.encode('utf-8'))

            elif splited_data[j] == "#4":

                print("데이터 출력 시작")
                isActive = True
                if isActive:
                    print(splited_datas[i]);
                    print(splited_data[14])
                    sender = splited_data[16]
                    timeMill = splited_data[14]
                    machine = splited_data[15]
                    print("데이터 분석 코드 삽입 및 처리 : " + sender)
                    print("데이터 기기 :  " + machine)
                    w = float(splited_data[13])
                    r = float(splited_data[9])
                    RM = w * (r*0.1)
                    senddata = "#8,"+str(round(RM))+","+sender+","+timeMill+","+machine+";";
                    print(RM)
                    client_socket.send(senddata.encode('utf-8'))
                    isActive = False
                    pass