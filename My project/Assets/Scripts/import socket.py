import socket

HOST = '127.0.0.1'  # IP of the Unity machine
PORT = 5005         # Must match Unity's port

def handle_color(color_hex):
    print(f"🎨 Robot should switch to color #{color_hex}")
    # You could convert it to RGB if needed:
    rgb = tuple(int(color_hex[i:i+2], 16) for i in (0, 2, 4))
    print(f"🟥 RGB value: {rgb}")
    # Add robot command logic here...

def main():
    try:
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            print(f"🔌 Connecting to {HOST}:{PORT}...")
            s.connect((HOST, PORT))
            print("✅ Connected! Waiting for messages...")

            buffer = b""
            while True:
                data = s.recv(1024)
                if not data:
                    print("⚠️ Connection closed by Unity.")
                    break

                buffer += data
                while b'\n' in buffer:
                    line, buffer = buffer.split(b'\n', 1)
                    message = line.decode('ascii').strip()
                    print(f"📥 Received: {message}")

                    if message.startswith("COLOR:"):
                        color_hex = message.split("COLOR:")[1]
                        handle_color(color_hex)
                    else:
                        # Handle coordinates or other messages
                        print(f"🖊️ Interpreting as coordinates: {message}")

    except ConnectionRefusedError:
        print("❌ Could not connect to Unity — is it running?")
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    main()
