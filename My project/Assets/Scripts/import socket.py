import socket

HOST = '127.0.0.1'  # IP of the machine running Unity (use actual IP if on another device)
PORT = 5005         # Must match the port Unity is using

def main():
    try:
        # Create TCP socket
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
            print(f"🔌 Connecting to Unity at {HOST}:{PORT}...")
            s.connect((HOST, PORT))
            print("✅ Connected! Listening for messages...")

            # Keep receiving data from Unity
            buffer = b""
            while True:
                data = s.recv(1024)
                if not data:
                    print("⚠️ Connection closed by Unity.")
                    break

                buffer += data
                while b'\n' in buffer:
                    line, buffer = buffer.split(b'\n', 1)
                    print(f"📥 Received: {line.decode('ascii').strip()}")

    except ConnectionRefusedError:
        print("❌ Connection refused. Make sure Unity is running and listening on port 5005.")
    except Exception as e:
        print(f"❌ Error: {e}")

if __name__ == "__main__":
    main()
