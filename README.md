# MP Modbus App

**MP Modbus App** is an advanced Modbus Master diagnostic tool designed for monitoring, testing, and managing industrial devices communicating via the Modbus protocol. The application is built using .NET 8 (Windows Forms).

The program allows for easy connection to devices, register reading, real-time data visualization on charts, and network traffic logging.

## 🚀 Key Features

### 1. Communication and Connections
The application supports various transmission media and protocol modes:
* **Modbus RTU / ASCII (Serial Port):** Full configuration of COM port parameters (Baud Rate, Data Bits, Parity, Stop Bits).
* **Modbus TCP/IP:** Standard communication via Ethernet.
* **Modbus RTU over TCP:** Tunneling of RTU frames over a TCP/IP network.
    > ⚠️ **WARNING:** The **RTU over TCP** mode is currently experimental and has not been fully tested. Use with caution.

### 2. Device Management (Database)
* **SQLite Database:** All device configurations, register groups, and variable definitions are saved in a local SQLite database.
* **Device Tree:** Clear navigation structure (Device -> Measurement Group).
* **Import / Export:** Ability to export device definitions to XML files and import them, facilitating configuration transfer between workstations.

### 3. Data Reading and Visualization
* **Register Type Support:**
    * Coils (0x)
    * Discrete Inputs (1x)
    * Input Registers (3x)
    * Holding Registers (4x)
* **Measurement Tables:** Displaying values in Decimal, Hexadecimal (HEX), and Binary formats.
* **Real-Time Charts:** Each measurement group has a dedicated chart tab, allowing for live trend tracking of selected registers.

### 4. Advanced Diagnostic Tools
* **Intelligent Address Scanner:**
    * Automatic scanning of a specified address range.
    * **Data Type Detection:** The algorithm automatically attempts to read 16, 32, 48, and 64-bit blocks. In case of a read error for a smaller type, the program attempts to read a wider data type, allowing for automatic mapping of variables (e.g., float, long) without manual configuration.
    * Visualization of variable size in results (e.g., `(32b)`, `(64b)`).
* **Device Scanner:** Bus scanning to find active Slave IDs.
* DISCLAIMER: This scanning function broadcasts queries to arbitrary addresses, which may disrupt sensitive or legacy hardware. This tool should only be used in a controlled environment. The developer accepts no liability for any damages or network interruptions caused by the use of this feature.

* **Communication Log (Traffic Monitor):** View sent (TX) and received (RX) frames in raw mode (HEX) with timestamps, which is crucial for debugging transmission issues.

## 🛠️ System Requirements
* Operating System: Windows 10/11
* Platform: .NET 8.0 Runtime

## 📝 License
This project is licensed under the MIT License.

---
*Author: [Marcin Pindel / JuniorSoft]*