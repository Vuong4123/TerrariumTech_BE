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
    ImageUrl: str
    TerrariumImages: list[str]

@app.post("/predict", response_model=AITerrariumResponse)
def predict(request: AITerrariumRequest):
    image_pool = {
        "default": [
            "https://cdn2.tuoitre.vn/thumb_w/1200/2022/11/2/terrarium3-read-only-16673534182271088929882.jpg",
            "https://cdn2.tuoitre.vn/thumb_w/1200/2022/11/2/terrarium3-read-only-16673534182271088929882.jpg",
            "https://cdn2.tuoitre.vn/thumb_w/1200/2022/11/2/terrarium3-read-only-16673534182271088929882.jpg"
        ]
    }

    selected_images = random.sample(image_pool["default"], k=2)

    return AITerrariumResponse(
        TerrariumName=f"AI_Terrarium_{random.randint(1000,9999)}",
        Description="Terrarium được sinh ra từ AI",
        MinPrice=random.uniform(100, 300),
        MaxPrice=random.uniform(301, 500),
        Stock=random.randint(5, 20),
        ImageUrl=selected_images[0],
        TerrariumImages=selected_images
    )
