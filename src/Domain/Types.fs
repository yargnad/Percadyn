namespace Percadyn.Domain

/// Represents the state of a single cell in the cellular automata.
type CellState =
    | Alive
    | Dead

    /// Helper to toggle state
    member this.Toggle() =
        match this with
        | Alive -> Dead
        | Dead -> Alive

/// A 2x2 neighborhood of cells used in the Margolus neighborhood.
type Block =
    { TopLeft: CellState
      TopRight: CellState
      BottomLeft: CellState
      BottomRight: CellState }

/// The entire grid of the simulation.
/// Using a 2D array for performance and ease of indexing.
type Grid = CellState array array
