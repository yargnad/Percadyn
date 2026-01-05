namespace Percadyn.Domain


module Rules =

    /// A Rule is a function that transforms a Block into another Block.
    /// In the Margolus neighborhood, these rules must be reversible (bijective)
    /// to be "Time-Symmetric", allowing for conservation of "information".
    type Rule = Block -> Block

    /// Sample Rule: Single Rotation.
    /// Acts on the block as if it were a billiard ball collision or rotation.
    /// This is a common Margolus rule.
    /// Logic: 
    /// - If 1 cell is alive: Rotate Clockwise (CW)
    /// - If 3 cells are alive: Rotate Counter-Clockwise (CCW) (or CW, symmetry depends on variant)
    /// - If 2 cells are alive (diagonal): Keep static (or swap, depending on rule)
    /// - If 2 cells are alive (adjacent): Move forward
    /// Here we implement a standard "Billiard Ball" like logic or simple rotation.
    /// Let's implement "Single Rotation" (also known as the "Critters" rule component or similar).
    /// For this scaffolding, we'll implement a simplified version:
    /// Rotate CW if 1 cell is active. Rotate CCW if 3 cells are active.
    /// Invert if 2 diagonal cells.
    /// Identity otherwise (0, 4, or 2 adjacent).
    let singleRotation (block: Block) : Block =
        // Count alive cells
        let cells = [ block.TopLeft; block.TopRight; block.BottomLeft; block.BottomRight ]
        let count = cells |> List.filter (function | Alive -> true | Dead -> false) |> List.length

        match count with
        | 1 -> 
            // Rotate CW
            { TopLeft = block.BottomLeft
              TopRight = block.TopLeft
              BottomRight = block.TopRight
              BottomLeft = block.BottomRight }
        | 3 ->
            // Rotate CCW (or CW depending on symmetry preference, let's do CCW for balance)
            { TopLeft = block.TopRight
              TopRight = block.BottomRight
              BottomRight = block.BottomLeft
              BottomLeft = block.TopLeft }
        | 2 ->
            // Check for diagonal
            match block.TopLeft, block.BottomRight with
            | Alive, Alive -> // Diagonal \
                 // Invert or Rotate? Billiard Ball machine usually preserves. 
                 // Let's pass through for now or rotate 180.
                 // Let's Rotate 180 (Swap diagonals)
                 { TopLeft = block.BottomRight
                   TopRight = block.BottomLeft
                   BottomRight = block.TopLeft
                   BottomLeft = block.TopRight }
            | _ -> 
                match block.TopRight, block.BottomLeft with
                | Alive, Alive -> // Diagonal /
                     { TopLeft = block.BottomRight
                       TopRight = block.BottomLeft
                       BottomRight = block.TopLeft
                       BottomLeft = block.TopRight }
                | _ -> block // Adjacent - Identity
        | _ -> block // 0 or 4 - Identity

(*
    Margolus Neighborhood Logic:
    
    The grid is divided into 2x2 blocks.
    Even Steps (t=0, 2...): Grid is partitioned starting at (0,0).
    Odd Steps (t=1, 3...): Grid is partitioned starting at (1,1).
*)

    /// Splits the grid into 2x2 blocks based on the time step (phase).
    /// If step is even, offset is 0. If odd, offset is 1.
    /// Returns a jagged array of Blocks.
    let partition (grid: Grid) (step: int) : Block[][] =
        let rows = grid.Length
        let cols = if rows > 0 then grid.[0].Length else 0
        
        let offset = if step % 2 = 0 then 0 else 1
        
        // Calculate number of blocks
        // We ensure we don't go out of bounds. 
        // With offset 1, we ignore the first and last row/col effectively if they don't fit.
        // Actually, margolus usually wraps around (toroidal), but for simplicity:
        // We will just process the available 2x2 chunks.
        let blockRows = (rows - offset) / 2
        let blockCols = (cols - offset) / 2
        
        Array.init blockRows (fun r ->
            Array.init blockCols (fun c ->
                let rStart = (r * 2) + offset
                let cStart = (c * 2) + offset
                {
                    TopLeft = grid.[rStart].[cStart]
                    TopRight = grid.[rStart].[cStart + 1]
                    BottomLeft = grid.[rStart + 1].[cStart]
                    BottomRight = grid.[rStart + 1].[cStart + 1]
                }
            )
        )

    /// Reassembles the blocks back into the grid.
    /// Needs the original grid to preserve the "edges" that weren't part of a block
    /// during the offset phase.
    let reassemble (blocks: Block[][]) (step: int) (currentGrid: Grid) : Grid =
        let rows = currentGrid.Length
        let cols = if rows > 0 then currentGrid.[0].Length else 0
        let offset = if step % 2 = 0 then 0 else 1
        
        // Copy current grid to preserve edges (mutable copy for performance in setting logic, then return immutable-ish view if needed, 
        // strictly speaking Grid is array array so it matches)
        // In F#, array copy:
        let newGrid = Array.init rows (fun r -> Array.copy currentGrid.[r])

        let blockRows = blocks.Length
        let blockCols = if blockRows > 0 then blocks.[0].Length else 0

        for r in 0 .. blockRows - 1 do
            for c in 0 .. blockCols - 1 do
                let block = blocks.[r].[c]
                let rStart = (r * 2) + offset
                let cStart = (c * 2) + offset
                
                newGrid.[rStart].[cStart] <- block.TopLeft
                newGrid.[rStart].[cStart + 1] <- block.TopRight
                newGrid.[rStart + 1].[cStart] <- block.BottomLeft
                newGrid.[rStart + 1].[cStart + 1] <- block.BottomRight

        newGrid
