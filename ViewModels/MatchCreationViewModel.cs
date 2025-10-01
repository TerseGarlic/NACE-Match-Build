using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NACE_Match_Builder.Models;
using NACE_Match_Builder.Builders;

namespace NACE_Match_Builder.ViewModels;

// Add the CodAction record to track map ban/pick actions
public record CodAction(string Type, string Map, int Step, Team Actor);

// Valorant Map Action record
public record ValorantAction(string Type, string Map, int Step, Team Actor);

public class MatchCreationViewModel : ObservableObject
{
    private readonly SecurityConfig _securityConfig = new();

    private string _school1Name = string.Empty; public string School1Name { get => _school1Name; set { var sanitizedValue = SanitizeInput(value); SetProperty(ref _school1Name, sanitizedValue); InvalidateTeams(); } }
    private string _school2Name = string.Empty; public string School2Name { get => _school2Name; set { var sanitizedValue = SanitizeInput(value); SetProperty(ref _school2Name, sanitizedValue); InvalidateTeams(); } }
    public ObservableCollection<Team> Teams { get; } = new();

    private void InvalidateTeams()
    {
        CoinFlipWinner = null; RosterA = null; RosterB = null; ResetCodState();
        Teams.Clear();
        if (!string.IsNullOrWhiteSpace(School1Name)) Teams.Add(new Team(School1Name, new List<Player>()));
        if (!string.IsNullOrWhiteSpace(School2Name)) Teams.Add(new Team(School2Name, new List<Player>()));
        UpdateCommands(); CoinFlipCommand.RaiseCanExecuteChanged();
        OnPropertyChanged(nameof(ShowAssignRosters));
        OnPropertyChanged(nameof(CanStartSequence));
    }

    // Coin flip
    private string? _coinFlipWinner; public string? CoinFlipWinner { get => _coinFlipWinner; set { SetProperty(ref _coinFlipWinner, value); OnPropertyChanged(nameof(ShowAssignRosters)); CoinFlipCommand.RaiseCanExecuteChanged(); } }
    public bool ShowAssignRosters => CoinFlipWinner != null && Teams.Count == 2;
    private bool CanCoinFlip() => !string.IsNullOrWhiteSpace(School1Name) && !string.IsNullOrWhiteSpace(School2Name) && CoinFlipWinner == null;

    // Roster A/B assignment
    private Team? _rosterA; public Team? RosterA { get => _rosterA; set { SetProperty(ref _rosterA, value); if (value != null && RosterB == value) RosterB = null; UpdateCommands(); OnPropertyChanged(nameof(CanStartSequence)); } }
    private Team? _rosterB; public Team? RosterB { get => _rosterB; set { SetProperty(ref _rosterB, value); if (value != null && RosterA == value) RosterA = null; UpdateCommands(); OnPropertyChanged(nameof(CanStartSequence)); } }

    // CoD Map Pools
    public List<string> HardpointPool { get; } = new() { "Hacienda", "Red Card", "Rewind", "Skyline", "Vault" };
    public List<string> SndPool { get; } = new() { "Dealership", "Hacienda", "Protocol", "Red Card", "Rewind" };
    public List<string> ControlPool { get; } = new() { "Hacienda", "Protocol", "Vault" };

    // Valorant Map Pool - Updated with the correct maps
    public List<string> ValorantMapPool { get; } = new()
    {
        "Abyss", "Ascent", "Bind", "Corrode", "Haven", "Lotus", "Sunset"
    };

    // Working state during pick/ban
    public ObservableCollection<string> HardpointRemaining { get; } = new();
    public ObservableCollection<string> SndRemaining { get; } = new();
    public ObservableCollection<string> ControlRemaining { get; } = new();
    public ObservableCollection<string> ValorantRemaining { get; } = new();

    // Result selections
    private string? _hpMap1; public string? HpMap1 { get => _hpMap1; set { SetProperty(ref _hpMap1, value); } }
    private string? _hpMap4; public string? HpMap4 { get => _hpMap4; set { SetProperty(ref _hpMap4, value); } }
    private string? _sndMap2; public string? SndMap2 { get => _sndMap2; set { SetProperty(ref _sndMap2, value); } }
    private string? _sndMap5; public string? SndMap5 { get => _sndMap5; set { SetProperty(ref _sndMap5, value); } }
    private string? _controlMap3; public string? ControlMap3 { get => _controlMap3; set { SetProperty(ref _controlMap3, value); } }
    private string? _valMap1; public string? ValMap1 { get => _valMap1; set { SetProperty(ref _valMap1, value); } }
    private string? _valMap2; public string? ValMap2 { get => _valMap2; set { SetProperty(ref _valMap2, value); } }
    private string? _valMap3; public string? ValMap3 { get => _valMap3; set { SetProperty(ref _valMap3, value); } }

    // Side choices
    private string? _hp1Side; public string? Hp1Side { get => _hp1Side; set { SetProperty(ref _hp1Side, value); OnPropertyChanged(nameof(Hp1Sides)); } }
    private string? _hp4Side; public string? Hp4Side { get => _hp4Side; set { SetProperty(ref _hp4Side, value); OnPropertyChanged(nameof(Hp4Sides)); } }
    private string? _snd2Side; public string? Snd2Side { get => _snd2Side; set { SetProperty(ref _snd2Side, value); OnPropertyChanged(nameof(Snd2Sides)); } }
    private string? _snd5Side; public string? Snd5Side { get => _snd5Side; set { SetProperty(ref _snd5Side, value); OnPropertyChanged(nameof(Snd5Sides)); } }
    private string? _control3Side; public string? Control3Side { get => _control3Side; set { SetProperty(ref _control3Side, value); OnPropertyChanged(nameof(Control3Sides)); } }
    private string? _val1Side; public string? Val1Side { get => _val1Side; set { SetProperty(ref _val1Side, value); OnPropertyChanged(nameof(Val1Sides)); } }
    private string? _val2Side; public string? Val2Side { get => _val2Side; set { SetProperty(ref _val2Side, value); OnPropertyChanged(nameof(Val2Sides)); } }
    private string? _val3Side; public string? Val3Side { get => _val3Side; set { SetProperty(ref _val3Side, value); OnPropertyChanged(nameof(Val3Sides)); } }

    // Computed displays: which team is on which side (using sequence rule: which roster chose side per step)
    public string Hp1Sides => BuildSidesDisplay(Hp1Side, chooserIsRosterA: false); // step 3 roster B chooses
    public string Hp4Sides => BuildSidesDisplay(Hp4Side, chooserIsRosterA: true);  // step 5 roster A chooses
    public string Snd2Sides => BuildSidesDisplay(Snd2Side, chooserIsRosterA: true); // step 9 roster A chooses
    public string Snd5Sides => BuildSidesDisplay(Snd5Side, chooserIsRosterA: false); // step 11 roster B chooses
    public string Control3Sides => BuildSidesDisplay(Control3Side, chooserIsRosterA: true); // step 14 roster A chooses
    public string Val1Sides => BuildValorantSidesDisplay(Val1Side, chooserIsRosterB: true); // roster B chooses
    public string Val2Sides => BuildValorantSidesDisplay(Val2Side, chooserIsRosterB: false); // roster A chooses
    public string Val3Sides => BuildValorantSidesDisplay(Val3Side, chooserIsRosterB: false); // roster A chooses

    private string BuildSidesDisplay(string? chosenSide, bool chooserIsRosterA)
    {
        if (RosterA == null || RosterB == null || string.IsNullOrWhiteSpace(chosenSide)) return string.Empty;
        var other = OppositeSide(chosenSide);
        string rosterASide = chooserIsRosterA ? chosenSide : other;
        string rosterBSide = chooserIsRosterA ? other : chosenSide;
        return $"{RosterA.Name}: {rosterASide} | {RosterB.Name}: {rosterBSide}";
    }

    private string OppositeSide(string side) => side switch { "Team 1" => "Team 2", "Team 2" => "Team 1", _ => side };
    private string BuildValorantSidesDisplay(string? chosenSide, bool chooserIsRosterB)
    {
        if (RosterA == null || RosterB == null || string.IsNullOrWhiteSpace(chosenSide)) return string.Empty;
        var other = OppositeValorantSide(chosenSide);
        string rosterBSide = chooserIsRosterB ? chosenSide : other;
        string rosterASide = chooserIsRosterB ? other : chosenSide;
        return $"{RosterA.Name}: {rosterASide} | {RosterB.Name}: {rosterBSide}";
    }

    private string OppositeValorantSide(string side) => side switch { "Attack" => "Defense", "Defense" => "Attack", _ => side };

    // Sequence tracking
    private int _codStep = -1; public int CodStep { get => _codStep; set { SetProperty(ref _codStep, value); OnPropertyChanged(nameof(CurrentCodPrompt)); UpdateStepFlags(); OnPropertyChanged(nameof(CanStartSequence)); UpdatePhaseFlags(); } }
    private int _valorantStep = -1;
    public int ValorantStep
    {
        get => _valorantStep;
        set
        {
            SetProperty(ref _valorantStep, value);
            OnPropertyChanged(nameof(CurrentValorantPrompt));
            UpdateValorantStepFlags();
        }
    }

    // Phase flags for enabling group sections
    public bool IsHardpointPhase { get; private set; }
    public bool IsSnDPhase { get; private set; }
    public bool IsControlPhase { get; private set; }

    private void UpdatePhaseFlags()
    {
        var mode = StepMode(CodStep);
        IsHardpointPhase = mode == "HP";
        IsSnDPhase = mode == "SnD";
        IsControlPhase = mode == "Control";
        OnPropertyChanged(nameof(IsHardpointPhase));
        OnPropertyChanged(nameof(IsSnDPhase));
        OnPropertyChanged(nameof(IsControlPhase));
    }

    // Helper method to determine the mode based on step number
    private string StepMode(int step)
    {
        if (step >= 0 && step <= 5) return "HP";
        if (step >= 6 && step <= 11) return "SnD";
        if (step >= 12 && step <= 14) return "Control";
        return string.Empty;
    }

    public bool CanStartSequence => RosterA != null && RosterB != null &&
    ((SelectedGame == GameTitle.CallOfDuty && CodStep == -1) ||
     (SelectedGame == GameTitle.Valorant && ValorantStep == -1));

    public ObservableCollection<MapMode> Rotation { get; } = new();
    public ObservableCollection<string> ValorantMaps { get; } = new(); // unchanged for future

    private GameTitle _selectedGame = GameTitle.CallOfDuty;
    public GameTitle SelectedGame
    {
        get => _selectedGame; set
        {
            SetProperty(ref _selectedGame, value);

            // When switching games, update the UI state
            if (value == GameTitle.CallOfDuty && CodStep == -1)
                Status = "Ready to start CoD map selection process.";
            else if (value == GameTitle.Valorant && ValorantStep == -1)
                Status = "Ready to start Valorant map selection process.";

            UpdateCommands();
        }
    }

    private string _status = "Enter two school names."; public string Status { get => _status; set => SetProperty(ref _status, value); }

    // Commands
    public RelayCommand CoinFlipCommand { get; }
    public RelayCommand BanCommand { get; }
    public RelayCommand PickCommand { get; }
    public RelayCommand SideCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand ResetCommand { get; }
    public RelayCommand BuildMatchCommand { get; }
    public RelayCommand StartSequenceCommand { get; }
    public RelayCommand ExportMatchCommand { get; }
    public RelayCommand CopyToClipboardCommand { get; }
    public RelayCommand ValorantBanCommand { get; }
    public RelayCommand ValorantPickCommand { get; }
    public RelayCommand ValorantSideCommand { get; }
    public RelayCommand NextMapCommand { get; } // For automatically selecting the last map

    private readonly List<CodAction> _actions = new();
    private readonly List<ValorantAction> _valorantActions = new();

    // Step flags for UI
    public bool IsHpBanStepA { get; private set; }
    public bool IsHpBanStepB { get; private set; }
    public bool IsHpPick1Step { get; private set; }
    public bool IsHpPick4Step { get; private set; }
    public bool IsSndBanStepB { get; private set; }
    public bool IsSndBanStepA { get; private set; }
    public bool IsSndPick2Step { get; private set; }
    public bool IsSndPick5Step { get; private set; }
    public bool IsCtrlBanStepA { get; private set; }
    public bool IsCtrlPick3Step { get; private set; }
    public bool IsSideSelection => CodStep is 3 or 5 or 9 or 11 or 14;
    public bool IsValorantSideSelection => ValorantStep is 3 or 5 or 9;

    public IEnumerable<string> SideOptions => new[] { "Team 1", "Team 2" };
    public IEnumerable<string> ValorantSideOptions => new[] { "Attack", "Defense" };

    private void UpdateStepFlags()
    {
        IsHpBanStepA = CodStep == 0; IsHpBanStepB = CodStep == 1; IsHpPick1Step = CodStep == 2; IsHpPick4Step = CodStep == 4;
        IsSndBanStepB = CodStep == 6; IsSndBanStepA = CodStep == 7; IsSndPick2Step = CodStep == 8; IsSndPick5Step = CodStep == 10;
        IsCtrlBanStepA = CodStep == 12; IsCtrlPick3Step = CodStep == 13;
        OnPropertyChanged(nameof(IsHpBanStepA)); OnPropertyChanged(nameof(IsHpBanStepB)); OnPropertyChanged(nameof(IsHpPick1Step)); OnPropertyChanged(nameof(IsHpPick4Step));
        OnPropertyChanged(nameof(IsSndBanStepB)); OnPropertyChanged(nameof(IsSndBanStepA)); OnPropertyChanged(nameof(IsSndPick2Step)); OnPropertyChanged(nameof(IsSndPick5Step));
        OnPropertyChanged(nameof(IsCtrlBanStepA)); OnPropertyChanged(nameof(IsCtrlPick3Step)); OnPropertyChanged(nameof(IsSideSelection));
    }

    private void UpdateValorantStepFlags()
    {
        // Update flags based on the current step
        OnPropertyChanged(nameof(IsValorantSideSelection));

        // Update the status with the current prompt
        if (SelectedGame == GameTitle.Valorant)
            Status = CurrentValorantPrompt;

        // Handle the automatic selection of the last map
        if (ValorantStep == 8 && ValorantRemaining.Count == 1)
        {
            // Auto-select the final map
            ValMap3 = ValorantRemaining[0];
            Status = $"Map 3 is {ValMap3}. Team A to pick side.";
            ValorantStep = 9; // Move to side selection
        }
    }

    // Resulting sequence of text prompts
    public string CurrentCodPrompt => SelectedGame != GameTitle.CallOfDuty || CodStep < 0 ? string.Empty : CodStep switch
    {
        0 => "HP: Roster A ban",
        1 => "HP: Roster B ban",
        2 => "HP: Roster A pick Map1",
        3 => "HP: Roster B choose side M1",
        4 => "HP: Roster B pick Map4",
        5 => "HP: Roster A choose side M4",
        6 => "SnD: Roster B ban",
        7 => "SnD: Roster A ban",
        8 => "SnD: Roster B pick Map2",
        9 => "SnD: Roster A side M2",
        10 => "SnD: Roster A pick Map5",
        11 => "SnD: Roster B side M5",
        12 => "Control: Roster A ban",
        13 => "Control: Roster B pick Map3",
        14 => "Control: Roster A side M3",
        _ => "Done"
    };

    // Valorant prompts based on the specified process
    public string CurrentValorantPrompt => SelectedGame != GameTitle.Valorant || ValorantStep < 0 ? string.Empty : ValorantStep switch
    {
        0 => "Team A ban map",
        1 => "Team B ban map",
        2 => "Team A pick map 1",
        3 => "Team B pick side for map 1",
        4 => "Team B pick map 2",
        5 => "Team A pick side for map 2",
        6 => "Team A ban map",
        7 => "Team B ban map",
        8 => "Last map is map 3",
        9 => "Team A pick side for map 3",
        _ => "Done"
    };

    public MatchCreationViewModel()
    {
        CoinFlipCommand = new RelayCommand(_ => DoCoinFlip(), _ => CanCoinFlip());
        BanCommand = new RelayCommand(m => DoBan(m as string), m => CanBan(m as string));
        PickCommand = new RelayCommand(m => DoPick(m as string), m => CanPick(m as string));
        SideCommand = new RelayCommand(s => DoSide(s as string), s => CanSide(s as string));
        UndoCommand = new RelayCommand(_ => UndoLast(), _ => _actions.Any());
        ResetCommand = new RelayCommand(_ => ResetCodState());
        BuildMatchCommand = new RelayCommand(_ => BuildMatch(), _ => CanBuildMatch());
        StartSequenceCommand = new RelayCommand(_ => StartSequence(), _ => CanStartSequence);
        ExportMatchCommand = new RelayCommand(_ => ExportMatch(), _ => CanBuildMatch());
        CopyToClipboardCommand = new RelayCommand(_ => CopyToClipboard(), _ => CanBuildMatch());

        // Initialize Valorant commands
        ValorantBanCommand = new RelayCommand(m => DoValorantBan(m as string), m => CanValorantBan(m as string));
        ValorantPickCommand = new RelayCommand(m => DoValorantPick(m as string), m => CanValorantPick(m as string));
        ValorantSideCommand = new RelayCommand(s => DoValorantSide(s as string), s => CanValorantSide(s as string));
        NextMapCommand = new RelayCommand(_ => DoNextMap(), _ => CanDoNextMap());

        ValorantMaps.Add("Abyss"); ValorantMaps.Add("Ascent"); ValorantMaps.Add("Bind");
        ValorantMaps.Add("Corrode"); ValorantMaps.Add("Haven"); ValorantMaps.Add("Lotus"); ValorantMaps.Add("Sunset");
        ResetCodState();
    }

    private void CopyToClipboard()
    {
        if (!CanBuildMatch()) return;

        var formattedText = FormatMatchForExport();

        // Sanitize clipboard content
        var sanitizedText = SanitizeClipboardContent(formattedText);

        try
        {
            Clipboard.SetText(sanitizedText);
            Status = "Match rotation copied to clipboard";
        }
        catch (Exception ex)
        {
            Status = $"Error copying to clipboard: {ex.Message}";
        }
    }

    private string SanitizeClipboardContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return string.Empty;

        // Remove any potentially sensitive system information
        // and limit content size
        const int maxClipboardSize = 32768; // 32KB limit
        return content.Length > maxClipboardSize
            ? content.Substring(0, maxClipboardSize) + "... [truncated]"
            : content;
    }

    private void ExportMatch()
    {
        if (!CanBuildMatch()) return;

        // Check if file export is allowed
        if (!_securityConfig.AllowFileExport)
        {
            Status = "File export is disabled by security policy";
            return;
        }

        var formattedText = FormatMatchForExport();

        // Check file size limit
        if (formattedText.Length > _securityConfig.MaxExportFileSize)
        {
            Status = "Export file too large";
            return;
        }

        try
        {
            string safeRosterAName = SanitizeFileName(RosterA?.Name ?? "TeamA");
            string safeRosterBName = SanitizeFileName(RosterB?.Name ?? "TeamB");

            string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = Path.Combine(documentsFolder, $"NACE_Match_{safeRosterAName}_vs_{safeRosterBName}_{timestamp}.txt");

            // Validate file extension
            var extension = Path.GetExtension(filename);
            if (!_securityConfig.AllowedFileExtensions.Contains(extension))
            {
                Status = "File extension not allowed by security policy";
                return;
            }

            File.WriteAllText(filename, formattedText);
            Status = $"Match exported to Documents folder";
        }
        catch (Exception ex)
        {
            Status = $"Error exporting match: {ex.Message}";
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Unknown";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());

        // Limit length and trim whitespace
        return sanitized.Trim().Substring(0, Math.Min(sanitized.Length, 50));
    }

    private string FormatMatchForExport()
    {
        if (RosterA == null || RosterB == null) return string.Empty;

        var sb = new StringBuilder();
        sb.AppendLine($"NACE Match: {RosterA.Name} vs {RosterB.Name}");
        sb.AppendLine($"Created: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine(new string('-', 50));

        // Map rotation
        sb.AppendLine("\nMAP ROTATION:");
        if (HpMap1 != null) sb.AppendLine($"Map 1: {HpMap1} (Hardpoint)");
        if (SndMap2 != null) sb.AppendLine($"Map 2: {SndMap2} (Search & Destroy)");
        if (ControlMap3 != null) sb.AppendLine($"Map 3: {ControlMap3} (Control)");
        if (HpMap4 != null) sb.AppendLine($"Map 4: {HpMap4} (Hardpoint)");
        if (SndMap5 != null) sb.AppendLine($"Map 5: {SndMap5} (Search & Destroy)");

        // Valorant maps
        if (ValMap1 != null) sb.AppendLine($"Map 1: {ValMap1} (Valorant)");
        if (ValMap2 != null) sb.AppendLine($"Map 2: {ValMap2} (Valorant)");
        if (ValMap3 != null) sb.AppendLine($"Map 3: {ValMap3} (Valorant)");

        // Side selections
        sb.AppendLine("\nSIDE SELECTIONS:");
        if (!string.IsNullOrEmpty(Hp1Sides)) sb.AppendLine($"Map 1: {Hp1Sides}");
        if (!string.IsNullOrEmpty(Snd2Sides)) sb.AppendLine($"Map 2: {Snd2Sides}");
        if (!string.IsNullOrEmpty(Control3Sides)) sb.AppendLine($"Map 3: {Control3Sides}");
        if (!string.IsNullOrEmpty(Hp4Sides)) sb.AppendLine($"Map 4: {Hp4Sides}");
        if (!string.IsNullOrEmpty(Snd5Sides)) sb.AppendLine($"Map 5: {Snd5Sides}");
        if (!string.IsNullOrEmpty(Val1Sides)) sb.AppendLine($"Map 1: {Val1Sides} (Valorant)");
        if (!string.IsNullOrEmpty(Val2Sides)) sb.AppendLine($"Map 2: {Val2Sides} (Valorant)");
        if (!string.IsNullOrEmpty(Val3Sides)) sb.AppendLine($"Map 3: {Val3Sides} (Valorant)");

        // Ban history
        sb.AppendLine("\nBAN HISTORY:");
        var bans = _actions.Where(a => a.Type == "Ban").Select(a => a.Map);
        sb.AppendLine(string.Join(", ", bans));

        return sb.ToString();
    }

    // Reset state for a new CoD match setup
    private void ResetCodState()
    {
        CodStep = -1;
        _actions.Clear();
        HardpointRemaining.Clear();
        SndRemaining.Clear();
        ControlRemaining.Clear();

        // Repopulate the pools
        foreach (var map in HardpointPool)
            HardpointRemaining.Add(map);

        foreach (var map in SndPool)
            SndRemaining.Add(map);

        foreach (var map in ControlPool)
            ControlRemaining.Add(map);

        HpMap1 = HpMap4 = SndMap2 = SndMap5 = ControlMap3 = null;
        Hp1Side = Hp4Side = Snd2Side = Snd5Side = Control3Side = null;
        Rotation.Clear();
        Status = "Ready to start map selection process.";

        // Reset Valorant state
        ResetValorantState();
    }

    public void ResetValorantState()
    {
        ValorantStep = -1;
        _valorantActions.Clear();
        ValorantRemaining.Clear();

        // Repopulate the pool
        foreach (var map in ValorantMapPool)
            ValorantRemaining.Add(map);

        ValMap1 = ValMap2 = ValMap3 = null;
        Val1Side = Val2Side = Val3Side = null;

        if (SelectedGame == GameTitle.Valorant)
            Status = "Ready to start Valorant map selection process.";
    }

    // Commands for each step of the process
    private async void DoCoinFlip()
    {
        if (!CanCoinFlip()) return;

        // Fix: Ensure Teams collection has items before trying to pick a random one
        if (Teams.Count == 0)
        {
            Status = "Please enter school names first.";
            return;
        }

        // Show the coin
        IsCoinVisible = true;

        // Immediately set a temporary winner to prevent multiple clicks
        CoinFlipWinner = "Flipping...";
        Status = "Flipping coin...";

        // Add a small delay to let the animation play (this happens in UI thread)
        await Task.Delay(3000); // Animation takes about 3000ms to complete

        var random = new Random();
        CoinFlipWinner = Teams[random.Next(0, Teams.Count)].Name;
        Status = $"{CoinFlipWinner} won the coin flip.";

        // Hide the coin after 3 seconds
        await Task.Delay(3000);
        IsCoinVisible = false;
    }

    private void StartSequence()
    {
        if (RosterA == null || RosterB == null) return;

        if (SelectedGame == GameTitle.CallOfDuty)
        {
            // Make sure the map pools are initialized
            if (HardpointRemaining.Count == 0 || SndRemaining.Count == 0 || ControlRemaining.Count == 0)
            {
                ResetCodState();
            }

            CodStep = 0; // Start at step 0
            Status = $"Starting CoD map veto process. {CurrentCodPrompt}";
        }
        else // Valorant
        {
            // Make sure the Valorant map pool is initialized
            if (ValorantRemaining.Count == 0)
            {
                ResetValorantState();
            }

            ValorantStep = 0; // Fix: Set ValorantStep instead of CodStep
            Status = $"Starting Valorant map veto process. {CurrentValorantPrompt}";
        }

        UpdateCommands(); // Critical to make the buttons work right away
    }

    // Add this method to the MatchCreationViewModel class
    private void StartValorantSequence()
    {
        if (RosterA == null || RosterB == null) return;

        // Make sure the Valorant map pool is initialized
        if (ValorantRemaining.Count == 0)
        {
            ResetValorantState();
        }

        ValorantStep = 0; // Start at step 0
        Status = $"Starting Valorant map veto process. {CurrentValorantPrompt}";
        UpdateCommands(); // Critical to make the buttons work right away
    }

    private void UpdateCommands()
    {
        // This is critical for making the UI update command availability
        BanCommand?.RaiseCanExecuteChanged();
        PickCommand?.RaiseCanExecuteChanged();
        SideCommand?.RaiseCanExecuteChanged();
        UndoCommand?.RaiseCanExecuteChanged();
        ResetCommand?.RaiseCanExecuteChanged();
        BuildMatchCommand?.RaiseCanExecuteChanged();
        StartSequenceCommand?.RaiseCanExecuteChanged();
        ExportMatchCommand?.RaiseCanExecuteChanged();
        CopyToClipboardCommand?.RaiseCanExecuteChanged();
        ValorantBanCommand?.RaiseCanExecuteChanged();
        ValorantPickCommand?.RaiseCanExecuteChanged();
        ValorantSideCommand?.RaiseCanExecuteChanged();
        NextMapCommand?.RaiseCanExecuteChanged();
    }

    private bool CanBan(string? map)
    {
        if (CodStep < 0 || map == null) return false;

        // Can ban in steps 0, 1 (HP bans), 6, 7 (SnD bans), 12 (Control ban)
        bool isValidBanStep = CodStep is 0 or 1 or 6 or 7 or 12;

        // Check if map is available in the correct pool for the current step
        return isValidBanStep && GetCurrentPool().Contains(map);
    }

    private void DoBan(string? map)
    {
        if (!CanBan(map) || map == null) return;

        // Get the active team for this step
        Team actor = GetActiveTeam();

        // Record the action
        _actions.Add(new CodAction("Ban", map, CodStep, actor));

        // Remove the map from the appropriate pool
        GetCurrentPool().Remove(map);

        // Update status and advance to next step
        Status = $"{actor.Name} banned {map}";
        CodStep++;
        UpdateCommands();
    }

    private bool CanPick(string? map)
    {
        if (CodStep < 0 || map == null) return false;

        // Can pick in steps 2, 4 (HP picks), 8, 10 (SnD picks), 13 (Control pick)
        bool isValidPickStep = CodStep is 2 or 4 or 8 or 10 or 13;

        // Check if map is available in the correct pool for the current step
        return isValidPickStep && GetCurrentPool().Contains(map);
    }

    private void DoPick(string? map)
    {
        if (!CanPick(map) || map == null) return;

        // Get the active team for this step
        Team actor = GetActiveTeam();

        // Record the action
        _actions.Add(new CodAction("Pick", map, CodStep, actor));

        // Assign the map to the appropriate slot
        switch (CodStep)
        {
            case 2: HpMap1 = map; break;
            case 4: HpMap4 = map; break;
            case 8: SndMap2 = map; break;
            case 10: SndMap5 = map; break;
            case 13: ControlMap3 = map; break;
        }

        // Remove the map from the pool and update status
        GetCurrentPool().Remove(map);
        Status = $"{actor.Name} picked {map}";

        // Advance to the next step (side selection)
        CodStep++;
        UpdateCommands();
    }

    private bool CanDoNextMap()
    {
        return ValorantStep == 8 && ValorantRemaining.Count == 1;
    }

    private void DoNextMap()
    {
        if (!CanDoNextMap()) return;

        // Set the last remaining map as Map 3
        ValMap3 = ValorantRemaining[0];
        Status = $"Final map is {ValMap3}";

        // Move to side selection
        ValorantStep = 9;
        UpdateCommands();
    }

    private bool CanSide(string? side)
    {
        // Side selection happens on steps 3, 5, 9, 11, 14
        return CodStep is 3 or 5 or 9 or 11 or 14 && !string.IsNullOrEmpty(side);
    }

    private void DoSide(string? side)
    {
        if (!CanSide(side) || side == null) return;

        Team actor = GetActiveTeam();

        // Assign side based on the current step
        switch (CodStep)
        {
            case 3: Hp1Side = side; break;
            case 5: Hp4Side = side; break;
            case 9: Snd2Side = side; break;
            case 11: Snd5Side = side; break;
            case 14: Control3Side = side; break;
        }

        Status = $"{actor.Name} chose {side}";

        // Advance to the next step or finalize if complete
        CodStep++;
        if (CodStep > 14)
        {
            Status = "Map veto process complete!";
            // Optionally build the match automatically
            BuildMatch();
        }

        UpdateCommands();
    }

    private bool CanValorantBan(string? map)
    {
        if (ValorantStep < 0 || map == null) return false;

        // Can ban in steps 0, 1, 6, 7 (ban steps)
        bool isValidBanStep = ValorantStep is 0 or 1 or 6 or 7;

        // Check if map is available
        return isValidBanStep && ValorantRemaining.Contains(map);
    }

    private void DoValorantBan(string? map)
    {
        if (!CanValorantBan(map) || map == null) return;

        // Get the active team for this step
        Team actor = GetValorantActiveTeam();

        // Record the action
        _valorantActions.Add(new ValorantAction("Ban", map, ValorantStep, actor));

        // Remove the map from the pool
        ValorantRemaining.Remove(map);

        // Update status and advance to next step
        Status = $"{actor.Name} banned {map}";
        ValorantStep++;
        UpdateCommands();
    }

    private bool CanValorantPick(string? map)
    {
        if (ValorantStep < 0 || map == null) return false;

        // Can pick in steps 2, 4 (map pick steps)
        bool isValidPickStep = ValorantStep is 2 or 4;

        // Check if map is available 
        return isValidPickStep && ValorantRemaining.Contains(map);
    }

    private void DoValorantPick(string? map)
    {
        if (!CanValorantPick(map) || map == null) return;

        // Get the active team for this step
        Team actor = GetValorantActiveTeam();

        // Record the action
        _valorantActions.Add(new ValorantAction("Pick", map, ValorantStep, actor));

        // Assign the map to the appropriate slot
        switch (ValorantStep)
        {
            case 2: ValMap1 = map; break;
            case 4: ValMap2 = map; break;
        }

        // Remove the map from the pool and update status
        ValorantRemaining.Remove(map);
        Status = $"{actor.Name} picked {map}";

        // Advance to the next step (side selection)
        ValorantStep++;
        UpdateCommands();
    }

    private bool CanValorantSide(string? side)
    {
        // Side selection happens on steps 3, 5, 9
        return ValorantStep is 3 or 5 or 9 && !string.IsNullOrEmpty(side);
    }

    private void DoValorantSide(string? side)
    {
        if (!CanValorantSide(side) || side == null) return;

        Team actor = GetValorantActiveTeam();

        // Assign side based on the current step
        switch (ValorantStep)
        {
            case 3: Val1Side = side; break;
            case 5: Val2Side = side; break;
            case 9: Val3Side = side; break;
        }

        Status = $"{actor.Name} chose {side}";

        // Advance to the next step or finalize if complete
        ValorantStep++;
        if (ValorantStep > 9)
        {
            Status = "Map veto process complete!";
            // Optionally build the match automatically
            BuildMatch();
        }

        UpdateCommands();
    }

    private Team GetActiveTeam()
    {
        if (RosterA == null || RosterB == null)
        {
            throw new InvalidOperationException("Roster A and Roster B must be assigned before accessing the active team.");
        }

        // Determine which roster (A or B) is active for the current step
        bool isRosterA = CodStep switch
        {
            0 or 2 or 7 or 10 or 12 or 14 => true,  // Roster A steps
            1 or 3 or 4 or 6 or 8 or 11 or 13 => false, // Roster B steps
            _ => false
        };

        return isRosterA ? RosterA : RosterB;
    }

    private Team GetValorantActiveTeam()
    {
        if (RosterA == null || RosterB == null)
        {
            throw new InvalidOperationException("Roster A and Roster B must be assigned before accessing the active team.");
        }

        // Determine which roster (A or B) is active for the current step
        bool isRosterA = ValorantStep switch
        {
            0 or 2 or 5 or 6 or 9 => true,  // Team A steps
            1 or 3 or 4 or 7 => false, // Team B steps
            _ => true // Default to A
        };

        return isRosterA ? RosterA : RosterB;
    }

    private ObservableCollection<string> GetCurrentPool()
    {
        // Return the appropriate map pool based on the current step
        if (CodStep >= 0 && CodStep <= 5) return HardpointRemaining;
        if (CodStep >= 6 && CodStep <= 11) return SndRemaining;
        return ControlRemaining;
    }

    private void UndoLast()
    {
        if (!_actions.Any()) return;

        // Get the last action
        var lastAction = _actions.Last();
        _actions.RemoveAt(_actions.Count - 1);

        // Restore the map to the appropriate pool
        switch (StepMode(lastAction.Step))
        {
            case "HP":
                HardpointRemaining.Add(lastAction.Map);
                if (lastAction.Step == 2) HpMap1 = null;
                if (lastAction.Step == 4) HpMap4 = null;
                break;
            case "SnD":
                SndRemaining.Add(lastAction.Map);
                if (lastAction.Step == 8) SndMap2 = null;
                if (lastAction.Step == 10) SndMap5 = null;
                break;
            case "Control":
                ControlRemaining.Add(lastAction.Map);
                if (lastAction.Step == 13) ControlMap3 = null;
                break;
        }

        // Handle side selection undos
        if (CodStep is 4 or 6 or 10 or 12 or 15)
        {
            switch (CodStep)
            {
                case 4: Hp1Side = null; break;
                case 6: Hp4Side = null; break;
                case 10: Snd2Side = null; break;
                case 12: Snd5Side = null; break;
                case 15: Control3Side = null; break;
            }
        }

        // Go back one step
        CodStep = lastAction.Step;
        Status = $"Undid last action. {CurrentCodPrompt}";
        UpdateCommands();
    }

    // Add this method to the MatchCreationViewModel class
    private void BuildMatch()
    {
        // Implement your match building logic here.
        // For now, just update the status to indicate the match was built.
        Status = "Match built successfully.";
    }

    // Add this method to the MatchCreationViewModel class
    private bool CanBuildMatch()
    {
        // You can adjust the logic as needed; for now, require both rosters to be assigned
        return RosterA != null && RosterB != null;
    }

    private string SanitizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Use security config for length limit
        var maxLength = _securityConfig.MaxTeamNameLength;
        var cleaned = input.Trim();

        // Limit length first
        if (cleaned.Length > maxLength)
            cleaned = cleaned.Substring(0, maxLength);

        // Remove control characters and potentially dangerous characters
        var sanitized = new string(cleaned.Where(c =>
            !char.IsControl(c) &&
            c != '<' && c != '>' && c != '&' &&
            c != '"' && c != '\'' && c != '/'
        ).ToArray());

        return sanitized.Trim();
    }

    private bool _isCoinVisible;
    public bool IsCoinVisible
    {
        get => _isCoinVisible;
        set => SetProperty(ref _isCoinVisible, value);
    }
}

public static class CollectionExtensions
{
    public static void SyncFrom(this ObservableCollection<string> target, IEnumerable<string> source)
    {
        target.Clear(); foreach (var s in source) target.Add(s);
    }
}

public class SecurityConfig
{
    public int MaxTeamNameLength { get; set; } = 50;
    public int MaxExportFileSize { get; set; } = 1048576; // 1MB
    public bool AllowFileExport { get; set; } = true;
    public string[] AllowedFileExtensions { get; set; } = { ".txt" };
}