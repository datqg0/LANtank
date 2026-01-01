# 🛠️ LANtank

**LANtank** is a simple 2D tank shooting game written in **C#**, built using a **Server–Client** architecture.

The project is designed mainly for **learning and experimentation with networking**, starting with **LAN multiplayer**, and gradually evolving toward **Online Multiplayer**.

---

## ✨ Features

* Multiplayer tank battle over LAN
* Authoritative server (server controls all game logic)
* Wall collision & bullet collision
* Health, damage, respawn, and scoring
* Simple JSON-based state synchronization
* Multiple clients can connect simultaneously

---

## 📦 Environment Requirements

* Windows 10 / 11
* **Visual Studio 2019 / 2022**
* .NET (Framework or .NET, depending on what you choose)
* Git (optional, for cloning the repository)

---

## 🚀 How to Run the SERVER

> ⚠️ **Important**
> There is **NO pre-built Server project** in this repository.
> You must **manually create a C# Console Application** and copy the server source code into it.

---

### 1️⃣ Create a C# Console Application

1. Open **Visual Studio**
2. Click **Create a new project**
3. Select **Console App**
4. Choose **C#**
5. Name the project (example: `LANtankServer`)
6. Select **.NET Framework** or **.NET**
7. Click **Create**

---

### 2️⃣ Add the Game Server Code

1. Open `Program.cs`
2. Remove all existing code
3. Copy the full **GameServer** source code from the repository
4. Paste it into `Program.cs`
5. Save the file

---

### 3️⃣ Run the Server

* Press **Start (F5)** or **Ctrl + F5**
* The console will display:

```text
Server IP: 192.168.1.10
PORT (default 3636):
```

* Press **Enter** to use the default port `3636`
* Or enter a custom port if needed

📌 **Remember the IP and Port** — the client needs them to connect.

---

## 🎮 How to Run the CLIENT

### 1️⃣ Open the Client Project

1. Open **Visual Studio**
2. `File → Open → Project/Solution`
3. Select the **Client / Game** project

---

### 2️⃣ Configure Server Address

In the client source code, set the server IP and port:

```csharp
string serverIP = "192.168.1.10";
int serverPort = 3636;
```

➡️ Change the IP to match the machine running the server.

---

### 3️⃣ Run the Game

* Press **Start (F5)** or **Ctrl + F5**
* Each running client controls **one tank**
* You can open **multiple clients on the same PC** for testing

---

## 🎯 Game Controls

| Key     | Action                     |
| ------- | -------------------------- |
| `W`     | Move up                    |
| `A`     | Move left                  |
| `S`     | Move down                  |
| `D`     | Move right                 |
| `SPACE` | Shoot                      |
| `ESC`   | Exit game (if implemented) |

---

## 🧠 Architecture Overview

### Server

* Handles all core game logic:

  * Tank movement
  * Collision detection (walls, tanks, bullets)
  * Shooting and damage
  * Respawning and scoring
* Maintains the **authoritative game state**
* Broadcasts game state to all clients using JSON

### Client

* Sends player input (`W`, `A`, `S`, `D`, `SPACE`) to the server
* Receives game state snapshots
* Renders tanks, bullets, and the map
* Does **not** control game logic

➡️ **The server is authoritative** — clients cannot cheat by modifying local state.

---

## 🧪 Testing Tips

* Disable Firewall if clients cannot connect
* Use `127.0.0.1` when running server and client on the same machine
* Use LAN IPs (`192.168.x.x`) when testing across multiple machines
* Open multiple client instances to simulate multiplayer

---

## 🔧 Future Development Plans

* 🌐 LAN → Online Multiplayer
* 📡 UDP or Hybrid networking
* 🧠 Client-side prediction & lag compensation
* 🤖 AI-controlled bots
* 🧱 Destructible environments
* 🎮 Lobby & room system

---

## 📄 License

This project is intended for **learning and research purposes**.
You are free to modify, extend, and experiment with the code.

---
