# NACE Match Builder

A comprehensive WPF application designed for the National Association of Collegiate Esports (NACE) to streamline competitive match creation and map veto processes for Call of Duty and Valorant tournaments.

## Features

### 🎮 Multi-Game Support
- **Call of Duty**: Full support for Hardpoint, Search & Destroy, and Control game modes
- **Valorant**: Complete map veto system with Attack/Defense side selection

### 🏆 Professional Tournament Management
- **Coin Flip System**: Built-in animated coin flip to determine starting advantage
- **Team Assignment**: Easy roster management with Team A/B designation
- **Step-by-Step Veto**: Guided pick/ban process following official tournament rules

### 📊 Map Pool Management
- **Call of Duty Maps**:
  - Hardpoint: Hacienda, Red Card, Rewind, Skyline, Vault
  - Search & Destroy: Dealership, Hacienda, Protocol, Red Card, Rewind
  - Control: Hacienda, Protocol, Vault
- **Valorant Maps**: Abyss, Ascent, Bind, Corrode, Haven, Lotus, Sunset

### 💾 Export & Sharing
- **File Export**: Save match details to timestamped text files
- **Clipboard Copy**: Quick copy for sharing match rotations
- **Security Features**: Input sanitization and file size limits

### 🎨 Modern Interface
- **Dark Theme**: Professional esports-ready UI
- **Real-time Updates**: Live status tracking throughout the veto process
- **Animated Elements**: Smooth coin flip animation and visual feedback
- **Responsive Design**: Clean layout with organized sections

## Installation

### Prerequisites
- Windows 10/11
- .NET 8.0 Runtime

### Download & Run
1. Download the latest release from the [Releases](https://github.com/TerseGarlic/NACE-Match-Builder/releases) page
2. Extract the ZIP file to your desired location
3. Run `NACE Match Builder.exe`

## Usage

### Basic Match Setup
1. **Select Game**: Choose between Call of Duty or Valorant tabs
2. **Enter Teams**: Input both school names
3. **Coin Flip**: Click "Flip Coin" to randomly determine starting team
4. **Assign Rosters**: Set which team is Roster A/B
5. **Start Process**: Click "Start Pick/Ban" to begin the veto sequence

### Veto Process
- Follow the on-screen prompts for each step
- **Ban**: Remove maps from consideration
- **Pick**: Select maps for the rotation
- **Side Selection**: Choose starting side for picked maps
- **Undo**: Reverse the last action if needed

### Export Options
- **Save to File**: Creates a formatted text file in your Documents folder
- **Copy to Clipboard**: Copies match details for easy sharing

## Map Veto Sequences

### Call of Duty (Best of 5)
1. HP Ban → HP Ban → HP Pick → Side Choice
2. HP Pick → Side Choice → SnD Ban → SnD Ban
3. SnD Pick → Side Choice → SnD Pick → Side Choice
4. Control Ban → Control Pick → Side Choice

### Valorant (Best of 3)
1. Ban → Ban → Pick → Side Choice
2. Pick → Side Choice → Ban → Ban
3. Last map auto-selected → Side Choice

## Technical Details

- **Framework**: .NET 8.0 WPF
- **Architecture**: MVVM Pattern
- **Language**: C# 12.0
- **Security**: Input sanitization, file validation, size limits

## Security Features

- Input length restrictions (50 characters max)
- File size limits (1MB max export)
- Path traversal protection
- Clipboard content sanitization
- Invalid character filtering

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For issues, questions, or feature requests, please open an issue on the [GitHub Issues](https://github.com/TerseGarlic/NACE-Match-Builder/issues) page.

## Acknowledgments

- Built for the National Association of Collegiate Esports (NACE)
- Designed to meet official tournament standards
- Created to streamline competitive match administration

---

**Built with ❤️ for the collegiate esports community**
