using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace Chess.Game {
	public class GameManager : MonoBehaviour {

		public enum Result { Playing, WhiteIsMated, BlackIsMated, Stalemate, Repetition, FiftyMoveRule, InsufficientMaterial , TimeExpired }

		public event System.Action onPositionLoaded;
		public event System.Action<Move> onMoveMade;

		public enum PlayerType { Human, AI }

		public bool loadCustomPosition;
		public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";
		public string customPosition2 = "4k3/8/8/8/4K3/8/8/8 w - - 0 1";
		public PlayerType whitePlayerType;
		public PlayerType blackPlayerType;
		public Color[] colors;

		public bool useClocks;
		public Clock whiteClock;
		public Clock blackClock;
		public TMPro.TMP_Text blackDiagnostics;
		public TMPro.TMP_Text whiteDiagnostics;
		public TMPro.TMP_Text resultUI;

		Result gameResult;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;
		List<Move> gameMoves;
		BoardUI boardUI;

		public ulong zobristDebug;
		public Board board { get; private set; }

        Board searchBoard; // Duplicate version of board used for ai search
		public bool blackIsBot;
		public bool whiteIsBot;
		public bool inCheck;
		public GameObject Clock;

		public AISettings whiteAISettings;
		public AISettings blackAISettings;

		public GameObject Message;
		public bool startGame;
		public GameObject filesRanks;


		public void Start() {
			initalize();
			startStartCoroutine();
		}

		public void startStartCoroutine() {
			StartCoroutine(waitTime());
		}

		private System.Collections.IEnumerator waitTime() {
			startGame = false;
			Message.GetComponent<Animator>().SetTrigger("Show");
			Message.GetComponent<Animator>().ResetTrigger("Hide");
			yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
			Message.GetComponent<Animator>().SetTrigger("Hide");
			Message.GetComponent<Animator>().ResetTrigger("Show");
			startGame = true;
			FindObjectOfType<audioManager>().Play("New Game");
			NotifyPlayerToMove();
		}

		public void initalize() {
			//Application.targetFrameRate = 60;
			boardUI = FindObjectOfType<BoardUI>();
			gameMoves = new List<Move>();
			board = new Board();
			searchBoard = new Board();
			//aiSettings.diagnostics = new Search.SearchDiagnostics();
			whiteAISettings.diagnostics = new Search.SearchDiagnostics();
			blackAISettings.diagnostics = new Search.SearchDiagnostics();
			translateInput();
			NewGame();
		}

		public void translateInput() {
			this.whitePlayerType = (this.whiteIsBot = startInfo.whiteIsBot) ? PlayerType.AI : PlayerType.Human;
			this.blackPlayerType = (this.blackIsBot = startInfo.blackIsBot) ? PlayerType.AI : PlayerType.Human;
			if (this.whitePlayerType == PlayerType.AI) {
				translateDifficulty(startInfo.whiteDifficulty, whiteAISettings);
				whiteDiagnostics.gameObject.SetActive(true);
			}
			if (this.blackPlayerType == PlayerType.AI) {
				translateDifficulty(startInfo.blackDifficulty, blackAISettings);
				blackDiagnostics.gameObject.SetActive(true);
			}
			Clock.SetActive(useClocks = !startInfo.isUnlimitedTime);
			if (!startInfo.isUnlimitedTime) {
				whiteClock.startSeconds = blackClock.startSeconds = startInfo.startSeconds * 60;
				whiteClock.bonusSeconds = blackClock.bonusSeconds = startInfo.bonusSeconds;
			}
		}

		private void translateDifficulty(int value, AISettings settings) {
			settings.useThreading = true;
			settings.useBook = true;
			settings.useIterativeDeepening = true;
			settings.useFixedDepthSearch = true;
			settings.useMoveOrdering = true;
			settings.useTranspositionTable = false;
			switch (value) {
				case 1:
					settings.depth = 1;
					settings.useMoveOrdering = false;
					break;
				case 2:
					settings.depth = 2;
					settings.useMoveOrdering = false;
					break;
				case 3:
					settings.depth = 3;
					settings.useMoveOrdering = false;
					break;
				case 4:
					settings.depth = 4;
					break;
				case 5:
					settings.depth = 5;
					settings.useTranspositionTable = true;
					settings.useFixedDepthSearch = false;
					break;
			}
		}

		void Update() {
			if (startGame) {
				if ((checkForTimeOver()) == Result.TimeExpired)
					PrintGameResult(gameResult = Result.TimeExpired);
				zobristDebug = board.ZobristKey;
				if (gameResult == Result.Playing) {
					LogAIDiagnostics();
					playerToMove.Update();
					if (useClocks) {
						whiteClock.isTurnToMove = board.WhiteToMove;
						blackClock.isTurnToMove = !board.WhiteToMove;
					}
				} else {
					stopClocks();
				}
				if (Input.GetKeyDown(KeyCode.E))
					ExportGame();
				if (Input.GetKey(KeyCode.UpArrow))
					invertBoard(true);
				else if (Input.GetKey(KeyCode.DownArrow))
					invertBoard(false);
				if (inCheck)
					boardUI.SelectCheckSquare(BoardRepresentation.CoordFromIndex(board.KingSquare[board.WhiteToMove ? 0 : 1]));
			}
		}
		public void invertBoard(bool prespective) {
			boardUI.SetPerspective(prespective);
			CanvasGroup[] objects = filesRanks.GetComponentsInChildren<CanvasGroup>();
			for (int i = boardUI.whiteIsBottom ? 1 : 8; boardUI.whiteIsBottom ? i <= 8 : i >= 1; i = boardUI.whiteIsBottom ? i + 1 : i - 1) {
				objects[0].gameObject.GetComponentsInChildren<TMPro.TMP_Text>()[boardUI.whiteIsBottom ? i - 1 : 8 - i].text = ((char)(96 + i)).ToString();
				objects[1].gameObject.GetComponentsInChildren<TMPro.TMP_Text>()[boardUI.whiteIsBottom ? i - 1 : 8 - i].text = ((9 - i)).ToString();
			}
			blackClock.gameObject.transform.position = new Vector3(blackClock.gameObject.transform.position.x, (boardUI.whiteIsBottom ? +1.42f : -1.42f));
			whiteClock.gameObject.transform.position = new Vector3(whiteClock.gameObject.transform.position.x, (boardUI.whiteIsBottom ? -1.42f : +1.42f));
			blackDiagnostics.gameObject.transform.position = new Vector3(blackDiagnostics.gameObject.transform.position.x, (boardUI.whiteIsBottom ? +2.714f : -2.714f));
			whiteDiagnostics.gameObject.transform.position = new Vector3(whiteDiagnostics.gameObject.transform.position.x, (boardUI.whiteIsBottom ? -2.714f : +2.714f));
			blackDiagnostics.alignment = boardUI.whiteIsBottom ? TMPro.TextAlignmentOptions.TopLeft : TMPro.TextAlignmentOptions.BottomLeft;
			whiteDiagnostics.alignment = boardUI.whiteIsBottom ? TMPro.TextAlignmentOptions.BottomLeft : TMPro.TextAlignmentOptions.TopLeft;
		}
		public void stopClocks() {
			whiteClock.isTurnToMove = false;
			blackClock.isTurnToMove = false;
		}


		void OnMoveChosen (Move move) {
			bool animateMove = playerToMove is AIPlayer;
			if (playerToMove is HumanPlayer)
				animateMove = !((HumanPlayer)playerToMove).dragging;
			board.MakeMove (move);
			string moveName = searchBoard.MakeMove (move);
			gameMoves.Add (move);
			onMoveMade?.Invoke (move);
			boardUI.OnMoveMade (board, move, animateMove);
			if (!NotifyPlayerToMove())
				FindObjectOfType<audioManager>().Play(moveName);
		}
		public void NewGame() {
			whiteClock.Reset();
			blackClock.Reset();
			inCheck = false;
			NewGame(whitePlayerType, blackPlayerType);
		}

		void NewGame (PlayerType whitePlayerType, PlayerType blackPlayerType) {
			gameMoves.Clear ();
			if (loadCustomPosition) {
				board.LoadPosition (customPosition2);
				searchBoard.LoadPosition (customPosition2);
			} else {
				board.LoadStartPosition ();
				searchBoard.LoadStartPosition ();
			}
			onPositionLoaded?.Invoke ();
			boardUI.UpdatePosition (board);
			boardUI.ResetSquareColours (false);

			CreatePlayer (ref whitePlayer, whitePlayerType , whitePlayerType == PlayerType.AI ? whiteAISettings : null);
			CreatePlayer (ref blackPlayer, blackPlayerType , blackPlayerType == PlayerType.AI ? blackAISettings : null);

			blackClock.isTurnToMove = false;
			whiteClock.isTurnToMove = false;


			gameResult = Result.Playing;
			PrintGameResult (gameResult);
			LogAIDiagnostics();
		}

		public void LogAIDiagnostics() {
			string text = "";
			//var d = aiSettings.diagnostics;
			AISettings settings = board.WhiteToMove ? whiteAISettings : blackAISettings;
			var d = settings.diagnostics;
			int noOfPositions = d.numPositionsEvaluated;
			text += $"<color=#{ColorUtility.ToHtmlStringRGB(colors[0])}>DEPTH SEARCHED: {(!settings.useIterativeDeepening?settings.depth:d.lastCompletedDepth)}";
			string evalString = "";
			if (d.isBook) {
				evalString = "BOOK";
			} else {
				float displayEval = d.eval / 100f;
				if (playerToMove is AIPlayer && !board.WhiteToMove) {
					displayEval = -displayEval;
				}
				evalString = ($"{displayEval:00.00}").Replace(",", ".");
				if (Search.IsMateScore(d.eval)) {
					evalString = $"MATE IN {Search.NumPlyToMateFromScore(d.eval)} MOVE";
				}
			}
			text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(colors[1])}>SCORE: {evalString}";
			text += $"\n<color=#{ColorUtility.ToHtmlStringRGB(colors[2])}>MOVE: {d.moveVal}";
			if (blackIsBot&& !board.WhiteToMove)
				blackDiagnostics.text = text;
			if (whiteIsBot && board.WhiteToMove)
				whiteDiagnostics.text = text;

		}

		public void ExportGame() {
			string pgn = PGNCreator.CreatePGN(gameMoves.ToArray());
			File.WriteAllText(("Moves.txt"), pgn);
		}

		public void Back () {
			StartCoroutine(changeScene());
		}

		private System.Collections.IEnumerator changeScene() {
			yield return new WaitForSeconds(1);
			SceneManager.LoadScene(0);
		}

		bool NotifyPlayerToMove () {
			inCheck = false;
			gameResult = GetGameState ();
			string results = PrintGameResult (gameResult);
			if (gameResult == Result.Playing) {
				playerToMove = (board.WhiteToMove) ? whitePlayer : blackPlayer;
				playerToMove.NotifyTurnToMove();
				LogAIDiagnostics();
				if (inCheck) {
					FindObjectOfType<audioManager>().Play("Check");
					return true;
				}
			} else {
				FindObjectOfType<audioManager>().Play(results);
				if (useClocks)
					stopClocks();
				return true;
			}
			return false;
		}

		string PrintGameResult(Result result) {
			float subtitleSize = resultUI.fontSize * 0.75f;
			string subtitleSettings = $"<color=#787878> <size={subtitleSize}>";
			string gameOverName = "";
			if (result == Result.Playing) {
				resultUI.text = ((board.WhiteToMove) ? "WHITE'S" : "BLACK'S") + " TURN.";
				resultUI.text += (inCheck ? "\n( CHECK )" : "");
			} else if (result == Result.WhiteIsMated || result == Result.BlackIsMated) {
				resultUI.text = "CHECKMATE";
				resultUI.text += subtitleSettings + "\n( " + (result == Result.WhiteIsMated ? "BLACK " : "WHITE ") + "WINS )";
				gameOverName = "Check Mate";
			} else if (result == Result.FiftyMoveRule) {
				resultUI.text = "DRAW";
				resultUI.text += subtitleSettings + "\n( 50 MOVE RULE )";
				gameOverName = "Game Over";
			} else if (result == Result.Repetition) {
				resultUI.text = "DRAW";
				resultUI.text += subtitleSettings + "\n( 3-FOLD REPETITION )";
				gameOverName = "Game Over";
			} else if (result == Result.Stalemate) {
				resultUI.text = "DRAW";
				resultUI.text += subtitleSettings + "\n( STALEMATE )";
				gameOverName = "Stale Mate";
			} else if (result == Result.InsufficientMaterial) {
				resultUI.text = "DRAW";
				resultUI.text += subtitleSettings + "\n( INSUFFUICIENT MATERIAL )";
				gameOverName = "Game Over";
			} else if (result == Result.TimeExpired) {
				resultUI.text = ((whiteClock.isTurnToMove ? "BLACK" : "WHITE") + " WINS");
				resultUI.text += subtitleSettings + "\n( TIME EXPIRED )";
				gameOverName = "Game Over";
			}
			return gameOverName;
		}

		Result GetGameState () {
			MoveGenerator moveGenerator = new MoveGenerator ();
			var moves = moveGenerator.GenerateMoves (board);
			inCheck = moveGenerator.InCheck();
			// Look for mate/stalemate
			if (moves.Count == 0) {
				if (moveGenerator.InCheck ()) {
					return (board.WhiteToMove) ? Result.WhiteIsMated : Result.BlackIsMated;
				}
				return Result.Stalemate;
			}

			// Fifty move rule
			if (board.fiftyMoveCounter >= 100) {
				return Result.FiftyMoveRule;
			}

			// Threefold repetition
			int repCount = board.RepetitionPositionHistory.Count ((x => x == board.ZobristKey));
			if (repCount == 3) {
				return Result.Repetition;
			}

			// Look for insufficient material (not all cases implemented yet)
			int numPawns = board.pawns[Board.WhiteIndex].Count + board.pawns[Board.BlackIndex].Count;
			int numRooks = board.rooks[Board.WhiteIndex].Count + board.rooks[Board.BlackIndex].Count;
			int numQueens = board.queens[Board.WhiteIndex].Count + board.queens[Board.BlackIndex].Count;
			int numKnights = board.knights[Board.WhiteIndex].Count + board.knights[Board.BlackIndex].Count;
			int numBishops = board.bishops[Board.WhiteIndex].Count + board.bishops[Board.BlackIndex].Count;

			if (numPawns + numRooks + numQueens == 0) {
				if (numKnights == 1 || numBishops == 1) {
					return Result.InsufficientMaterial;
				}
			}
			return Result.Playing;
		}

		private Result checkForTimeOver() {
			if (useClocks) {
				Clock currentClock = whiteClock.isTurnToMove ? whiteClock : blackClock;
				if (currentClock.secondsRemaining == 0)
					return Result.TimeExpired;
			}
			return Result.Playing;
		}

		void CreatePlayer (ref Player player, PlayerType playerType , AISettings aiSettings) {
			if (player != null)
				player.onMoveChosen -= OnMoveChosen;
			if (playerType == PlayerType.Human)
				player = new HumanPlayer (board);
			else
				player = new AIPlayer (searchBoard, aiSettings);
			player.onMoveChosen += OnMoveChosen;
		}

	}
}