# app.py
from fastapi import FastAPI
from pydantic import BaseModel
import random

app = FastAPI()

class AITerrariumRequest(BaseModel):
    EnvironmentId: int
    ShapeId: int
    TankMethodId: int

class AITerrariumResponse(BaseModel):
    TerrariumName: str
    Description: str
    MinPrice: float
    MaxPrice: float
    Stock: int

@app.post("/predict", response_model=AITerrariumResponse)
def predict(request: AITerrariumRequest):
    return AITerrariumResponse(
        TerrariumName=f"AI_Terrarium_{random.randint(1000,9999)}",
        Description="Generated based on AI-trained model",
        MinPrice=random.uniform(100, 300),
        MaxPrice=random.uniform(301, 500),
        Stock=random.randint(5, 20)
    )
