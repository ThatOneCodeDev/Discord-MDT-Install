# SentinelSec Studios - Discord Provisioning Utility

**Purpose:** Automate the installation and re-installation of Discord, ensuring it is operational on user systems. This utility is particularly useful in environments where Discord is critical and needs to be re-provisioned automatically after failures.

---

## Features

- **Automated Discord Installation**: Ensures Discord is installed on user systems.
- **Login Check**: Automatically checks for Discord installation at each login and re-installs it if missing or broken.
- **User and Machine-Wide Opt-Out Options**:
  - Users can opt out of the login check while still ensuring Discord is installed.
  - Administrators can disable the utility machine-wide.
- **Re-Enabling**: Easily re-enable the functionality using the `/install` switch.
- **Source Code Transparency**: Fully open-source for audit and improvement.

---

## Usage

### **Installation**

To configure the utility and enable login checks:
```bash
MDT-InstallDiscord.exe /install
```

- Downloads and configures the utility in the `C:\Users\Public` folder.
- Adds a login check to ensure Discord remains installed.

### **Login Check Behavior**

At each login, the utility will:
1. Check if Discord is installed for the current user.
2. If Discord is missing or broken, re-install it automatically.
3. Skip checks for users or machines that have opted out.

### **Opt-Out Options**

#### **Opt-Out for Current User**
To stop the utility from running at login for the current user:
```bash
MDT-InstallDiscord.exe /optout user
```

#### **Machine-Wide Opt-Out**
To stop the utility from running for all users on the machine:
```bash
MDT-InstallDiscord.exe /optout machine
```

- This removes all related registry entries for the utility.
- Administrators can re-enable the utility by running the `/install` command.

---

## Command-Line Arguments

| Argument           | Description                                                                                   |
|--------------------|-----------------------------------------------------------------------------------------------|
| `/install`         | Installs the utility, downloads it to `C:\Users\Public`, and configures login checks.         |
| `/check`           | Runs the utility in check mode to ensure Discord is installed (automatically triggered at login). |
| `/optout user`     | Disables login checks for the current user.                                                   |
| `/optout machine`  | Disables login checks for all users and cleans up related registry entries.                   |

---

## How It Works

1. **Installation**:
   - The utility is copied to `C:\Users\Public` and registered to run at login with the `/check` argument.
   - It ensures Discord is installed and operational for the user.

2. **Login Behavior**:
   - At each login, the utility checks for Discord’s presence.
   - If missing or broken, Discord is downloaded and installed automatically.

3. **Opt-Out**:
   - Users can disable the login check for themselves or administrators can opt out machine-wide.

4. **Re-Enabling**:
   - Running the `/install` argument re-enables the utility and login checks.

---

## Frequently Asked Questions

### **Why use this utility?**
Discord is often critical in collaborative or gaming environments. This utility ensures Discord is always available and operational, even if corrupted or uninstalled.

### **How do I re-enable login checks after opting out?**
Run the following command to reinstall the utility and re-enable login checks:
```bash
MDT-InstallDiscord.exe /install
```

### **What happens if I don’t have administrative rights?**
Without administrative rights:
- User-specific opt-outs (`/optout user`) can still be applied.
- Machine-wide options require administrator privileges.

### **Where is the source code?**
The full source code for this utility is available [here](https://github.com/ThatOneCodeDev/Discord-MDT-Install).

---

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes. For major changes, open an issue to discuss the proposed updates.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
