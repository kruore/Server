import tensorflow as tf
from tensorflow.keras import datasets, layers, models

gpus = tf.config.experimental.list_physical_devices('GPU')
if gpus:
    try:
        # Currently, memory growth needs to be the same across GPUs
        for gpu in gpus:
            tf.config.experimental.set_memory_growth(gpu, True)
    except RuntimeError as e:
        # Memory growth must be set before GPUs have been initialized
        print(e)

import numpy as np
import os
import csv
import glob
import fnmatch
from tqdm import tqdm
from numpy import int64
from numpy import array
from keras.models import Sequential
from keras.layers import Dense, LSTM
import pandas as pd
from numpy import asarray
from sklearn.preprocessing import MinMaxScaler
import matplotlib.pyplot as plt
from samplerate import resample
from scipy import signal
import librosa
from numpy import array
from keras.models import Sequential
from keras.layers import Dense, LSTM
import pandas as pd
from numpy import asarray
import matplotlib.pyplot as plt
from samplerate import resample
from scipy import signal
import librosa
from numpy import array
from keras.models import Sequential
from keras.layers import Dense, LSTM
import pandas as pd
from numpy import asarray
from sklearn.preprocessing import MinMaxScaler
import numpy as np
from numpy import array
from keras.models import Sequential
from keras.layers import Dense, LSTM
import pandas as pd
from numpy import asarray
from sklearn.model_selection import train_test_split
from sklearn.metrics import confusion_matrix, plot_confusion_matrix
from sklearn.metrics import classification_report
from keras.utils import to_categorical


path = 'D:\\PROJECT\\5_4 Server_code_test\\folder_template\\'

### READ ID csv
file_ID = 'D:\\PROJECT\\5_4 Server_code_test\\Voucher ID Number3.csv'
df_id = pd.read_csv(file_ID)
print(df_id.shape)

### TXT to CSV
# i1 = 0
# for (root, dirs, files) in os.walk(path):
#     for file in files:
#         txt_file_name = path + file
#         temp1 = pd.read_fwf(path+file)
#         csv_file_name = txt_file_name[:-4]+'.csv'
#         temp1.to_csv(csv_file_name)
#         i1 += 1
# exit()

### find min ptp time ###
airpot_sp_store = []; fcs_sp_store = []; watch_sp_store = [];
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        for (_, _, files) in os.walk(subpath):
            for file in files:
                print(file)
                file_path = subpath +'\\'+ file
                df = pd.read_csv(file_path)
                print(np.shape(df))
                if dir == '01_AIRPOT':
                    df_del_col = df.iloc[:, [1, 5, 6, 7, 8,9,10]] # ignore
                    airpot_sp_store.append(df_del_col.iloc[0,0])
                    df_del_col.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path +'\\'+dir+'\\'+ file)
                elif dir == '02_FCS':
                    df_del_col = df.iloc[:,[1,5,6,7,8]] # ignore
                    fcs_sp_store.append(df_del_col.iloc[0, 0])
                    df_del_col.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path+'\\'+dir+'\\' + file)
                elif dir == '03_WATCH':
                    df_del_col = df.iloc[:,[1,5,6,7,8,9,10,11]] # ignore
                    watch_sp_store.append(df_del_col.iloc[0, 0])
                    df_del_col.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path+'\\'+dir+'\\'+ file)

comb_store = airpot_sp_store+fcs_sp_store+watch_sp_store
comb_store=np.array(comb_store)
print(np.shape(comb_store))
comb_store = comb_store.reshape((-1,3), order = 'F')
B_min_ptp = np.min(comb_store,axis = 1)
print(B_min_ptp)

# del done
import pickle
with open('B_min_ptp', 'wb') as f:
    pickle.dump(B_min_ptp, f)

with open('B_min_ptp', 'rb') as f:
    B_min_ptp = pickle.load(f)


## Sampling Rate
Airpot_SR =15;
FCS_SR = 30;
Watch_SR =100;
window = 0.1;


#apply min_ptp coding...
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        for (_, _, files) in os.walk(subpath):
            i = 0
            for file in files:
                file_path = subpath +'\\'+ file
                print(file)
                df = pd.read_csv(file_path)
                if dir == '01_AIRPOT':
                    df['PTPTime'] = (df['PTPTime'] - B_min_ptp[i]) / 1000
                    df_del_col = df.iloc[:, [1, 5, 6, 7, 8, 9, 10]]
                    i += 1
                    #print(PTP_time_sub)
                    df_del_col.set_index('PTPTime', inplace=True)
                    df_del_col.to_csv(path +'\\'+dir+'\\'+ file)
                elif dir == '02_FCS':
                    print(file)
                    df['PTPTime'] = (df['PTPTime'] - B_min_ptp[i]) / 1000
                    df_del_col = df.iloc[:, [1, 5, 6, 7, 8]]
                    i += 1
                    #print(PTP_time_sub)
                    df_del_col.set_index('PTPTime', inplace=True)
                    df_del_col.to_csv(path + '\\' + dir + '\\' + file)

                elif dir == '03_WATCH':
                    df['PTPTime'] = (df['PTPTime'] - B_min_ptp[i]) / 1000
                    df_del_col = df.iloc[:, [1, 5, 6, 7, 8, 9, 10, 11]]
                    i += 1
                    #print(PTP_time_sub)
                    df_del_col.set_index('PTPTime', inplace=True)
                    df_del_col.to_csv(path + '\\' + dir + '\\' + file)


### find min ptp time ###
airpot_sp_store = []; fcs_sp_store = []; watch_sp_store = [];
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        for (_, _, files) in os.walk(subpath):
            for file in files:
                file_path = subpath +'\\'+ file
                df = pd.read_csv(file_path)
                if dir == '01_AIRPOT':

                    airpot_sp_store.append(df.iloc[0,0])
                    #df.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path +'\\'+dir+'\\'+ file)
                elif dir == '02_FCS':

                    fcs_sp_store.append(df.iloc[0, 0])
                    #df.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path+'\\'+dir+'\\' + file)
                elif dir == '03_WATCH':

                    watch_sp_store.append(df.iloc[0, 0])
                    #df.set_index('PTPTime', inplace=True)
                    #df_del_col.to_csv(path+'\\'+dir+'\\'+ file)

comb_store = airpot_sp_store+fcs_sp_store+watch_sp_store
comb_store=np.array(comb_store)
comb_store = comb_store.reshape((-1,3), order = 'F')
B_max_ptp = np.max(comb_store,axis = 1)
print(B_max_ptp)

# del done
import pickle
with open('B_max_ptp', 'wb') as f:
    pickle.dump(B_max_ptp, f)


import pickle
with open('B_max_ptp', 'rb') as f:
    B_max_ptp = pickle.load(f)

## Sampling Rate
Airpot_SR =15;
FCS_SR = 30;
Watch_SR =100;
window = 3;


#apply min_ptp coding...
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        for (_, _, files) in os.walk(subpath):
            i = 0
            for file in files:
                file_path = subpath +'\\'+ file
                df = pd.read_csv(file_path)
                if dir == '01_AIRPOT':
                    print(file)
                    col_name = df.columns
                    Arr_sp = df.to_numpy()
                    SP_row = np.where((Arr_sp[:, 0]-window < B_max_ptp[i]) & (B_max_ptp[i] < Arr_sp[:, 0]+window))
                    Arr_sp = Arr_sp[np.min(SP_row):,:]
                    # len_m=len(Arr_sp)
                    # print(len_m)
                    # Arr_sp1 = resample(Arr_sp, n_samples=len_m*4, random_state = 0)
                    Arr_sp1 = resample(Arr_sp, 4, 'sinc_best')
                    i += 1
                    df_scync = pd.DataFrame(Arr_sp1, columns=col_name)
                    df_scync.set_index('PTPTime', inplace=True)
                    df_scync.to_csv(path +'\\'+dir+'\\'+ file)
                elif dir == '02_FCS':
                    print(file)
                    col_name = df.columns
                    Arr_sp = df.to_numpy()
                    SP_row = np.where((Arr_sp[:, 0] - window < B_max_ptp[i]) & (B_max_ptp[i] < Arr_sp[:, 0] + window))
                    Arr_sp = Arr_sp[np.min(SP_row):, :]
                    len_m = len(Arr_sp)
                    #print(len_m)
                    Arr_sp1 = resample(Arr_sp, 2, 'sinc_best')
                    # Arr_sp1 = resample(Arr_sp, n_samples=round(len_m / 2), random_state=0)
                    i += 1
                    df_scync = pd.DataFrame(Arr_sp1, columns=col_name)
                    df_scync.set_index('PTPTime', inplace=True)
                    df_scync.to_csv(path + '\\' + dir + '\\' + file)
                elif dir == '03_WATCH':
                    print(file)
                    col_name = df.columns
                    Arr_sp = df.to_numpy()
                    SP_row = np.where((Arr_sp[:, 0] - window < B_max_ptp[i]) & (B_max_ptp[i] < Arr_sp[:, 0] + window))
                    Arr_sp = Arr_sp[np.min(SP_row):, :]
                    len_m = len(Arr_sp)
                    #print(len_m)
                    # Arr_sp1 = resample(Arr_sp, n_samples=round(len_m * 0.15), random_state=0)
                    Arr_sp1 = resample(Arr_sp, 0.15, 'sinc_best')
                    i += 1
                    df_scync = pd.DataFrame(Arr_sp1, columns=col_name)
                    df_scync.set_index('PTPTime', inplace=True)
                    df_scync.to_csv(path + '\\' + dir + '\\' + file)


### combine DATA

min_len_1 = []; min_len_2 =[]; min_len_3= [];
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        for (_, _, files) in os.walk(subpath):
            for file in files:
                file_path = subpath +'\\'+ file
                df = pd.read_csv(file_path)
                if dir == '01_AIRPOT':
                    min_len_1.append(len(df))
                elif dir == '02_FCS':
                    min_len_2.append(len(df))
                elif dir == '03_WATCH':
                    min_len_3.append(len(df))


min_len_1=np.array(min_len_1)
min_len_2=np.array(min_len_2)
min_len_3=np.array(min_len_3)
print(min_len_1.shape)
print(min_len_1.shape)
min_len12 = np.concatenate((min_len_1,min_len_2))
min_len = np.concatenate((min_len12,min_len_3))
min_len = min_len.reshape(-1,3,order='F')
Row_min = np.min(min_len,axis=1)

Vercat_01 = np.empty((0, 8))
Vercat_02 = np.empty((0, 5))
Vercat_03 = np.empty((0, 11))
### Combine Sensors ###
Combine_Table = [];
for (root, dirs, _) in os.walk(path):
    for dir in dirs:
        subpath = path+dir
        i = 0
        ii = 0
        for (_, _, files) in os.walk(subpath):

            for file in files:
                file_path = subpath +'\\'+ file
                df = pd.read_csv(file_path)

                if dir == '01_AIRPOT':
                    Arr_sp = df.iloc[1:Row_min[i], 0:7];
                    ID = file[15:17]
                    Arr_sp.insert(0, 'ID', int(ID))
                    Arr_data = Arr_sp.to_numpy()
                    i += 1
                    Vercat_01 = np.append(Vercat_01, Arr_data)
                    #Vercat_01 = Vercat_01.reshape(-1, 8, order='F')
                    Vercat_01 = Vercat_01.reshape(-1, 8)

                elif dir == '02_FCS':
                    Arr_sp = df.iloc[1:Row_min[i], :];
                    Arr_data = Arr_sp.to_numpy()
                    Vercat_02 = np.append(Vercat_02, Arr_data)
                    #Vercat_02 = Vercat_02.reshape(-1, 5, order='F')
                    Vercat_02 = Vercat_02.reshape(-1, 5)
                    i += 1

                elif dir == '03_WATCH':

                    print('file: ', file)
                    Arr_sp = df.iloc[1:Row_min[i], 0:8];
                    weigth = file[20:22]
                    equipment = file[18]
                    ID = file[15:17]
                    print('df_id[ID]: ', df_id['ID'])
                    print('int(ID)', int(ID))
                    ID_ind = df_id.loc[df_id['ID']==int(ID)]
                    ID_ind_arr = ID_ind.to_numpy()
                    print('here1: ', ID_ind_arr )
                    weight_1RM = ID_ind_arr[0][1]

                    Arr_sp['Weight']= int(weigth)
                    Arr_sp['Equipment'] = int(equipment)
                    Arr_sp['Weight_1RM'] = int(weight_1RM)
                    Arr_data = Arr_sp.to_numpy()

                    Vercat_03 = np.append(Vercat_03, Arr_data)
                    #Vercat_03 = Vercat_03.reshape(-1, 11, order='F')
                    Vercat_03 = Vercat_03.reshape(-1, 11)

                    i += 1


print(np.shape(Vercat_01))
print(np.shape(Vercat_02))
print(np.shape(Vercat_03))
Combine_Table = np.hstack((Vercat_01, Vercat_02[:,1:3], Vercat_03[:,1:]))
print('combine sensor shape1 :  ', np.shape(Combine_Table))
# column_names = ['ID', 'PTPTime', 'P_GyroX', 'P_GyroY','P_GyroZ','P_AccX','P_AccY','P_AccZ', 'F_DistanceMM','F_DistanceCM','F_Weight','F_Count','W_GyroX',
#                 'W_GyroY','W_GyroZ','W_AccX','W_AccY','W_AccZ','W_HeartRate','Weight', 'Equipment', 'weight_1RM']
column_names = ['ID', 'PTPTime', 'P_GyroX', 'P_GyroY','P_GyroZ','P_AccX','P_AccY','P_AccZ','F_DistanceMM', 'F_DistanceCM','W_GyroX',
                'W_GyroY','W_GyroZ','W_AccX','W_AccY','W_AccZ','W_HeartRate','Weight', 'Equipment', 'weight_1RM']


Combine_Table = pd.DataFrame(Combine_Table,columns = column_names)
Combine_Table.set_index('ID', inplace=True)
###Combine_Table.to_csv('Combine_Table1.csv')


def find_nearest(array, value):
    array = np.asarray(array)
    idx = (np.abs(array - value)).argmin()
    return array[idx]

window = 10   # /100
print('combine sensor shape2 :  ', np.shape(Combine_Table))
Dataset= Combine_Table
#Dataset = Dataset.iloc[:,:]
X = Dataset.iloc[:,:-1]
y = Dataset.iloc[:,-1]
print('Dataset should be 19', np.shape(Dataset))
print(np.shape(X))
print(np.shape(y))

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.33, random_state=42)

X_train_sh = np.shape(X_train)
X_test_sh = np.shape(X_test)
a,b =divmod(X_train_sh[0],100)
c,d =divmod(X_test_sh[0],100)

X_train = X_train.iloc[:-b,:]
y_train = y_train.iloc[:-b]
X_test = X_test.iloc[:-d,:]
y_test = y_test.iloc[:-d]


print('X_train', np.shape(X_train))
print('X_test', np.shape(X_test))
print('y_train', np.shape(y_train))
print('y_test', np.shape(y_test))


X_train =X_train.to_numpy()
y_train =y_train.to_numpy()
X_test =X_test.to_numpy()
y_test =y_test.to_numpy()


scaler = MinMaxScaler()
for ii in range(X_train_sh[1]):
    temp = np.expand_dims(X_train[:,ii], axis=1)
    temp = scaler.fit_transform(temp)
    X_train[:,ii] = temp.squeeze()

for ii in range(X_test_sh[1]):
    temp = np.expand_dims(X_test[:,ii], axis=1)
    temp = scaler.fit_transform(temp)
    X_test[:,ii] = temp.squeeze()



print(X_train.shape)
print(X_test.shape)


X_train =X_train.reshape(-1, 18, window)
X_test = X_test.reshape(-1, 18, window)

y_train = y_train.reshape(-1, window)
y_test = y_test.reshape(-1, window)

print('y_train unique len: ', np.unique(y_train))
print('y_test unique len: ', np.unique(y_test))


print(y_train.shape)
y_train_s = []
for i in range(len(y_train)):
    b = y_train[i,:]
    b_m =np.mean(b)
    b_i = find_nearest(b, b_m)
    y_train_s.append(b_i)

y_test_s = []
for i in range(len(y_test)):
    b = y_test[i,:]
    b_m =np.mean(b)
    b_i = find_nearest(b, b_m)
    y_test_s.append(b_i)



y_train =(np.max(y_train,1)).astype(int)
#
y_test = (np.max(y_test,1)).astype(int)


my_model = tf.keras.models.load_model("lstm_01")
y_pred = my_model.predict(X_test)
print('y_pred: ', y_pred)

