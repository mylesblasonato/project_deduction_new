from flask import Flask, request, jsonify
from transformers import pipeline

app = Flask(__name__)

# Load GPT-2 model (small version for fast performance)
generator = pipeline("text-generation", model="gpt2")

@app.route("/generate", methods=["POST"])
def generate():
    data = request.json
    prompt = data.get("prompt", "")

    if not prompt:
        return jsonify({"error": "No prompt provided"}), 400

    output = generator(prompt, max_length=100, num_return_sequences=1)[0]["generated_text"]
    return jsonify({"response": output})

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)
