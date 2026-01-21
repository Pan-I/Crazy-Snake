## 🔧 Contributing 💾

To contribute to Crazy Snake, please adhere to the following guidelines. Pull Requests (PRs) made directly to the `main` branch will be closed without merging.

### 🌳 Branching & History

This repository requires a linear history. Please rebase your branch against the appropriate **Trunk branch** (e.g., `PrimaryTrunk` or `SecondaryTrunk`) before submitting a PR. All PRs must be submitted to one of these Trunk branches. PRs directly from development branches or forks into `main` will not be accepted.

### 🛠 Environment Setup

The project utilizes both **.NET 8.0** and **.NET 10.0** (as specified in the developer guidelines). Before contributing logic or running tests, ensure your environment is correctly configured with both SDKs to avoid build or execution errors.

- For common setup problems, check [SETUP_ISSUES.md](readme_links/SETUP_ISSUES.md).


### 📁 Local Files & .gitignore

If you generate new types of files or folder structures intended only for your local build and not for the repository, please update the `.gitignore` accordingly.

**⚠️ CRITICAL: Do not remove the following from `.gitignore`:**
- `builds/`
- `/working_files`

### 📦 Assets Management

- **Assets Folder Content**: The `assets/` folder is reserved for game sprites, audio, and other resources. It is **not** intended for scene files (`.tscn`) or script files (`.cs`).
- **🚀 IMPORTANT**: Ensure you do not accidentally move or delete files within `assets/` unless intended, as they are now tracked by the repository.

*External contributions that include changes to assets will be subject to extra scrutiny.*

### 📝 Asset Attributions

Please update or create the necessary attribution files if you include resources that were downloaded or externally sourced rather than created by hand.