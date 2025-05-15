import socket

SERVER_IP = "172.20.10.2"  # <--- Inserisci l'IP del server
SERVER_PORT = 5005

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    print(f"Connessione a {SERVER_IP}:{SERVER_PORT}...")
    sock.connect((SERVER_IP, SERVER_PORT))
    print("Connesso. In attesa di messaggi...\n")

    buffer = b""
    while True:
        data = sock.recv(1024)
        if not data:
            print("Connessione chiusa dal server.")
            break

        buffer += data
        while b"\n" in buffer:
            line, buffer = buffer.split(b"\n", 1)
            print("Messaggio ricevuto:", line.decode().strip())

except Exception as e:
    print(f"Errore: {e}")
finally:
    sock.close()
    print("Socket chiuso.")