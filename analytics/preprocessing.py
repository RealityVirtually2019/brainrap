import numpy as np
import pandas as pd


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
label=label.reshape(-1,1)
oheLabel = OneHotEncoder()
label = oheLabel.fit_transform(label)

# feature scaling
from sklearn.preprocessing import StandardScaler
sc = StandardScaler()
sigs = sc.fit_transform(sigs)

import keras
from keras.layers import Dense, Dropout, Activation
from keras.models import Sequential

# creating a model
model = Sequential()
model.add(Dense(64, input_dim=sigs.shape[1]))
model.add(Activation('relu'))
model.add(Dense(32))
model.add(Activation('relu'))
model.add(Dense(3))
model.add(Activation('softmax'))

model.compile(loss='categorical_crossentropy', optimizer='adam', metrics=['accuracy'])

# training the model
model.fit(sigs, label, batch_size=10, epochs=10, validation_split=0.2)