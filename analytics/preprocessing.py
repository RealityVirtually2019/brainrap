import numpy as np
import pandas as pd
import matplotlib.pyplot as plt


# loading raw data
cols = ['Time','TRG','Eye_Time','Eye_X','Eye_Y','Trial','Num_Sequences','Sequence','Flash_Event']

dataset = pd.read_csv('raw_data.csv')
dataset = dataset.drop(cols, axis=1)
dataset = dataset[:].values
sigs = dataset[:,0:7]
label=dataset[:,7]

j = 6
for i in range(len(sigs)):
    if sigs[i,j] != '0' and sigs[i,j] != '-1':
            val = sigs[i,j]
            sigs[i,j]=int(val[1])

    if sigs[i,j] == '-1':
        sigs[i,j] = 0

for i in range(len(label)):
    if label[i] == -1:
        label[i] = 0

# encoding data and label
from sklearn.preprocessing import OneHotEncoder, LabelEncoder
ohe = OneHotEncoder(categorical_features=[6])
sigs = ohe.fit_transform(sigs).toarray()
""" label=label.reshape(-1,1)
oheLabel = OneHotEncoder()
label = oheLabel.fit_transform(label) """

# feature scaling
from sklearn.preprocessing import StandardScaler
sc = StandardScaler()
sigs = sc.fit_transform(sigs)

import keras
from keras.layers import Dense, Dropout, Activation
from keras.models import Sequential
from keras.callbacks import ModelCheckpoint, TensorBoard
from time import time
from keras.utils import plot_model

label = keras.utils.to_categorical(label, 5)

file_path = './train_model.ckpt'
callback_list = []

tb = TensorBoard(log_dir='logs/{}'.format(time()))
callback_list.append(tb)

# creating a model
model = Sequential()
model.add(Dense(64, input_dim=sigs.shape[1]))
model.add(Activation('relu'))
model.add(Dropout(0.2))
model.add(Dense(32))
model.add(Activation('relu'))
model.add(Dense(5))
model.add(Activation('softmax'))

model.compile(loss='categorical_crossentropy', optimizer='adam', metrics=['accuracy'])

ck = ModelCheckpoint(file_path, monitor='val_acc', verbose=1, save_best_only=True)
callback_list.append(ck)

# training the model
history = model.fit(sigs, label, batch_size=10, epochs=10, validation_split=0.2, verbose=1,callbacks=callback_list)

plot_model(model, to_file='model.png')

# Plot training & validation accuracy values
plt.plot(history.history['acc'])
plt.plot(history.history['val_acc'])
plt.title('Model accuracy')
plt.ylabel('Accuracy')
plt.xlabel('Epoch')
plt.legend(['Train', 'Test'], loc='upper left')
plt.show()

# Plot training & validation loss values
plt.plot(history.history['loss'])
plt.plot(history.history['val_loss'])
plt.title('Model loss')
plt.ylabel('Loss')
plt.xlabel('Epoch')
plt.legend(['Train', 'Test'], loc='upper left')
plt.show()