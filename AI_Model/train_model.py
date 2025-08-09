import pandas as pd
from sklearn.ensemble import RandomForestRegressor
from sklearn.model_selection import train_test_split
import pickle

# Load dữ liệu
df = pd.read_csv('terrarium_data.csv')
X = df[['EnvironmentId', 'ShapeId', 'TankMethodId']]
y = df[['MinPrice', 'MaxPrice', 'Stock']]

# Train model
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2)
model = RandomForestRegressor()
model.fit(X_train, y_train)

# Save model
with open('terrarium_model.pkl', 'wb') as f:
    pickle.dump(model, f)
