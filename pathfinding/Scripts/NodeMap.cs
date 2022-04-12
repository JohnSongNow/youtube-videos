using Godot;
using System;
using System.Collections.Generic;

public class NodeMap : Control
{
    public int width;
    public int height;
    public int rootIdx;
    public int dstIdx;
    public List<bool> map;
    public int seed;
    public bool useSeed = true;
    public bool animate = true;

    public Timer timer;
    public float searchTimestep = 0.001f;
    public float generateTimestep = 0.005f;
    public float pathTimestep = 0.001f;
    public GridContainer container;
    public Color wallColour = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    public Color mazeColour = new Color(0.9f, 0.9f, 0.9f, 1);
    public Color rootColour = new Color(1, 0, 0, 1);
    public Color dstColour = new Color(0, 1, 0, 1);
    public Color searchColour = new Color(1, 1, 0, 1);
    public Color expandedColour = new Color("#ff9f43");
    public Color pathColour = new Color(0.5f, 0.2f, 0.3f, 1);
    public Vector2 blockSize = new Vector2(5, 5);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        width = 79;
        height = 45;
        map = new List<bool>();
        timer = new Timer();
        AddChild(timer);
        AddUserSignal("finished");
        AddUserSignal("finished_generating");
        AddUserSignal("finished_pathfinding", new Godot.Collections.Array { this, new Godot.Collections.Array<int> (), new Godot.Collections.Array<int>() });

        container = GetNode("GridContainer") as GridContainer;
        container.SetColumns(width);

        for (int i = 0; i < width * height; i++)
        {
            ColorRect rect = new ColorRect();
            rect.SetFrameColor(wallColour);

            rect.SetCustomMinimumSize(blockSize);
            container.AddChild(rect);

            map.Add(false);
        }

        ResizeBlock();
    }

    public void SetHeight(int height)
    {
        // add more
        if (this.height < height)
        {
            for (int i = width * this.height; i < width * height; i++)
            {
                ColorRect rect = new ColorRect();
                rect.SetFrameColor(wallColour);

                rect.SetCustomMinimumSize(blockSize);
                container.AddChild(rect);

                map.Add(false);
            }
        }
        // remove more
        else
        {
            for (int i = width * height; i < width * this.height; i++)
            {
                container.GetChild(i).QueueFree();
            }
            map.RemoveRange(width * height, (this.height - height) * width);
        }

        this.height = height;
    }

    public void SetWidth(int width)
    {
        container.Columns = width;

        // add more
        if (this.width < width)
        {
            for (int i = this.width * height; i < width * height; i++)
            {
                ColorRect rect = new ColorRect();
                rect.SetFrameColor(wallColour);

                rect.SetCustomMinimumSize(blockSize);
                container.AddChild(rect);

                map.Add(false);
            }
        }
        // remove more
        else
        {
            for (int i = width * height; i < this.width * height; i++)
            {
                container.GetChild(i).QueueFree();
            }
            map.RemoveRange(width * height, height * (this.width - width));
        }

        this.width = width;
    }

    public void SetCell(int idx, bool walkable)
    {
        if (idx == rootIdx || idx == dstIdx)
        {
            return;
        }
        map[idx] = walkable;

        // colour the cell
        if (walkable)
        {
            (container.GetChild(idx) as ColorRect).SetFrameColor(mazeColour);
        }
        else
        {
            (container.GetChild(idx) as ColorRect).SetFrameColor(wallColour);

        }
    }

    public void ResizeBlock()
    {
        int hsep = (int)container.Get("custom_constants/hseparation");
        int vsep = (int)container.Get("custom_constants/vseparation");

        // assuming heps are always the same for now, since it has to be a square
        if (hsep ==  0)
        {
            blockSize.x = this.GetSize().x / width;
            blockSize.y = this.GetSize().y / height;
        }
        else
        {
            blockSize.x = (this.GetSize().x) / width - hsep;
            blockSize.y = (this.GetSize().y) / height - vsep;
        }
        
        for (int i = 0; i < width * height; i++)
        {
            ColorRect rect = container.GetChild(i) as ColorRect;      
            rect.SetCustomMinimumSize(blockSize);
        }
    }

    public void Clear()
    {
        rootIdx = 0;
        dstIdx = 0;
        for (int i = 0; i < height * width; i++)
        {
            map[i] = false;
            (container.GetChild(i) as ColorRect).Color = wallColour;
        }
    }

    public void SetDst(int idx)
    {
        if (idx != rootIdx && map[idx] == true)
        {
            RecolourCell(dstIdx);
            dstIdx = idx;
        }

        RecolourCell(dstIdx);
    }

    public void SetRoot(int idx)
    {
        if (idx != dstIdx && map[idx] == true)
        {
            RecolourCell(rootIdx);
            rootIdx = idx;
        }

        RecolourCell(rootIdx);
    }

    public void RecolourCell(int idx)
    {
        if (!map[idx])
        {
            (container.GetChild(idx) as ColorRect).SetFrameColor(wallColour);
        }
        else if (dstIdx == idx)
        {
            (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);
        }
        else if (rootIdx == idx)
        {
            (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);
        }
        else
        {
            (container.GetChild(idx) as ColorRect).SetFrameColor(mazeColour);
        }
    }

    /// <summary>
    /// Generates a nodemap with a path from src to dest
    /// </summary>
    public async void GenerateMap()
    {
        Random random;
        if (useSeed)
        {
            random = new Random(seed);
        }
        else
        {
            random = new Random();
        }
        
        // clear previous map
        map.Clear();
        
        // generate map now
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                map.Add(false);
            }
        }

        // nice random pls
        int rndX = random.Next();
        //rootIdx = random.Next(0, width * height / 2);
        rootIdx = (height - 2) * width + 1;
        dstIdx = rootIdx;

        // using randomized prim's
        // based on compass directions, 
        //    0
        //  3 r 1
        //    2
        // This is prim's algothirm for using blocks as walls instead
        List<int> frontierCells = new List<int>();
        map[rootIdx] = true;
        frontierCells.AddRange(GetFrontierCells(map, rootIdx, width, height, false));

        int count = 0;

        // colour map
        for (int i = 0; i < width * height; i++)
        {
            (container.GetChild(i) as ColorRect).SetFrameColor(wallColour);
        }

        // color dest and src
        (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);

        // used to trick the async sysetm for godot c#
        if (!animate) {
            timer.WaitTime = generateTimestep;
            timer.Start();
            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        while (frontierCells.Count > 0)
        {
            if (animate)
            {
                timer.WaitTime = generateTimestep;
                timer.Start();
            }

            count++;
            int nextCell =  random.Next(0, frontierCells.Count);
            int nextCellIdx = frontierCells[nextCell];
            List<int> neighbours = GetFrontierCells(map, nextCellIdx, width, height, true);

            // check for L blocks in all 4 corners,
            if (neighbours.Count > 0)
            {
                int neighbour = random.Next(0, neighbours.Count);
                int neighbourIdx = neighbours[neighbour];

                int diff = neighbourIdx - nextCellIdx;
                map[nextCellIdx] = true;
                map[nextCellIdx + (int)(diff / 2)] = true;
                (container.GetChild(nextCellIdx) as ColorRect).SetFrameColor(mazeColour);
                (container.GetChild(nextCellIdx + (int)(diff / 2)) as ColorRect).SetFrameColor(mazeColour);

                frontierCells.AddRange(GetFrontierCells(map, nextCellIdx, width, height, false));
            }

            frontierCells.RemoveAll(x => x == nextCellIdx);
            //dstIdx = nextCellIdx;

            if (animate)
            {
                await ToSignal(timer, "timeout");
                timer.Stop();
            } 
        }

        //
        /*
        List<int> remainingWalls = new List<int>();
        // making visited walls
        List<bool> visitedWalls = new List<bool>();
        for (int i = 0; i < 4 * width * height; i++)
        {
            visitedWalls.Add(false);
        }
        List<bool> visitedCells = new List<bool>();
        for (int i = 0; i < width * height; i++)
        {
            visitedCells.Add(false);
        }
        AddWalls(remainingWalls, visitedWalls, IdxToLoc(rootIdx, width));
        visitedCells[rootIdx] = true;
        map[rootIdx] = true;
        /*
        for (int i = 0; i < remainingWalls.Count; i++)
        {
            int wallIdx = remainingWalls[i];
            visitedWalls[wallIdx] = true;
            int side = wallIdx % 4;
            Vector2 idxPair = GetIdxPairFromWall(wallIdx, side, width, height);
            GD.Print(side + " " + idxPair);
        }
        while (remainingWalls.Count > 0)
        {
            int nextWall = random.Next(0, remainingWalls.Count);
            int wallIdx = remainingWalls[nextWall];
            visitedWalls[wallIdx] = true;
            int side = wallIdx % 4;
            Vector2 idxPair = GetIdxPairFromWall(wallIdx, side, width, height);

            // if only 1 is connected
            //if ((visitedCells[(int)idxPair.x] && !visitedCells[(int)idxPair.y]) || (!visitedCells[(int)idxPair.x] && visitedCells[(int)idxPair.y]))
            if (!map[(int)idxPair.y])
            {
                // mark this cell as visited
                // add 
                map[(int)idxPair.y] = true;
                AddWalls(remainingWalls, visitedWalls, IdxToLoc((int)idxPair.y, width));
                // add other cell walls
            }
            else
            {
                // remove other walls
                remainingWalls.RemoveAt(nextWall);
            }
        }
        */

        while (rootIdx == dstIdx || map[dstIdx] == false)
        {
            dstIdx = random.Next(0, width * height);
        }

        (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);

        EmitSignal("finished");
        EmitSignal("finished_generating");
    }

    public async void BFS()
    {
        Godot.Collections.Array<int> expanded = new Godot.Collections.Array<int>();
        Godot.Collections.Array<int> path = new Godot.Collections.Array<int>();
        // helps with path retracing,
        Dictionary<int, int> parent = new Dictionary<int, int>();
        List<bool> visited = new List<bool>();
        for (int i = 0; i < width * height; i++)
        {
            visited.Add(false);
        }
        visited[rootIdx] = true;

        Queue<int> q = new Queue<int>();
        q.Enqueue(rootIdx);
        while (q.Count > 0)
        {
            timer.WaitTime = searchTimestep;
            timer.Start();

            expanded.Add(q.Peek());

            int curr = q.Dequeue();
            if (curr == dstIdx)
            {
                break;
            }

            // colour expanded node
            if (curr != rootIdx)
            {
                (container.GetChild(curr) as ColorRect).SetFrameColor(expandedColour);
            }
            
            // queue the 4 directions
            foreach (int next in GetAdjacentNodes(curr, width, height))
            {
                if (!visited[next] && map[next])
                {
                    if (next != rootIdx && next != dstIdx)
                    {
                        (container.GetChild(next) as ColorRect).SetFrameColor(searchColour);
                    }
                    visited[next] = true;
                    parent[next] = curr;
                    q.Enqueue(next);
                }
            }

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        // reverse the path
        int currPath = dstIdx;
        while (parent.ContainsKey(currPath))
        {
            timer.WaitTime = pathTimestep;
            timer.Start();

            path.Add(currPath);

            if (currPath != dstIdx) {
                (container.GetChild(currPath) as ColorRect).SetFrameColor(pathColour);
            }
            parent.TryGetValue(currPath, out currPath);

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);
        (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);

        EmitSignal("finished");
        EmitSignal("finished_pathfinding", this, path, expanded);
    }

    public async void DFS()
    {
        // used for animation
        Godot.Collections.Array<int> expanded = new Godot.Collections.Array<int>();
        Godot.Collections.Array<int> path = new Godot.Collections.Array<int>();
        // helps with path retracing,
        Dictionary<int, int> parent = new Dictionary<int, int>();
        List<bool> visited = new List<bool>();
        for (int i = 0; i < width * height; i++)
        {
            visited.Add(false);
        }
        visited[rootIdx] = true;

        Stack<int> s = new Stack<int>();
        s.Push(rootIdx);

        while (s.Count > 0)
        {
            timer.WaitTime = searchTimestep;
            timer.Start();

            expanded.Add(s.Peek());

            int curr = s.Pop();
            if (curr == dstIdx)
            {
                break;
            }

            // colour expanded node
            if (curr != rootIdx && curr != dstIdx)
            {
                (container.GetChild(curr) as ColorRect).SetFrameColor(expandedColour);
            }

            // queue the 4 directions
            foreach (int next in GetAdjacentNodes(curr, width, height))
            {
                if (!visited[next] && map[next])
                {
                    if (next != rootIdx && next != dstIdx)
                    {
                        (container.GetChild(next) as ColorRect).SetFrameColor(searchColour);
                    }

                    visited[next] = true;
                    parent[next] = curr;
                    s.Push(next);
                }
            }

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        // reverse the path
        int currPath = dstIdx;
        while (parent.ContainsKey(currPath))
        {
            timer.WaitTime = pathTimestep;
            timer.Start();

            path.Add(currPath);

            if (currPath != rootIdx && currPath != dstIdx)
            {
                (container.GetChild(currPath) as ColorRect).SetFrameColor(pathColour);
            }
            parent.TryGetValue(currPath, out currPath);

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);
        (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);

        EmitSignal("finished");
        EmitSignal("finished_pathfinding", this, path, expanded);
    }

    public async void AStar()
    {
        Godot.Collections.Array<int> expanded = new Godot.Collections.Array<int>();
        Godot.Collections.Array<int> path = new Godot.Collections.Array<int>();

        HashSet<int> closed = new HashSet<int>();

        HashSet<int> open = new HashSet<int>();
        open.Add(rootIdx);

        Dictionary<int, int> parent = new Dictionary<int, int>();

        Dictionary<int, int> gScores = new Dictionary<int, int>();
        for (int i = 0; i < width * height; i++)
        {
            gScores[i] = int.MaxValue;
        }
        gScores[rootIdx] = 0;

        Dictionary<int, int> fScores = new Dictionary<int, int>();
        for (int i = 0; i < width * height; i++)
        {
            fScores[i] = int.MaxValue;
        }
        fScores[rootIdx] = ManDist(rootIdx, dstIdx, width);

        while (open.Count > 0)
        {
            timer.WaitTime = searchTimestep;
            timer.Start();

            // you can use a priority queue here, looping because its easy
            int lowest = int.MaxValue;
            int curr = 0;
            foreach (int key in open)
            {
                if (fScores[key] <= lowest)
                {
                    lowest = fScores[key];
                    curr = key;
                }
            }

            expanded.Add(curr);

            if (curr == dstIdx)
            {
                break;
            }

            if (curr != rootIdx && curr != dstIdx)
            {
                (container.GetChild(curr) as ColorRect).SetFrameColor(expandedColour);
            }

            open.Remove(curr);
            closed.Add(curr);
            
            // queue the 4 directions
            foreach (int next in GetAdjacentNodes(curr, width, height))
            {
                if (!map[next])
                {
                    continue;
                }

                if (closed.Contains(next))
                {
                    continue;
                }

                // dist is always 1
                int gScore = gScores[curr] + 1;

                if (!open.Contains(next))
                {
                    open.Add(next);

                    if (next != rootIdx && next != dstIdx)
                    {
                        (container.GetChild(next) as ColorRect).SetFrameColor(searchColour);
                    }
                }

                if (gScore < gScores[next])
                {
                    parent[next] = curr;
                    gScores[next] = gScore;
                    fScores[next] = gScores[next] + ManDist(next, dstIdx, width);
                }
            }

            await ToSignal(timer, "timeout");
            timer.Stop();
        }
        
        int currPath = dstIdx;
        while (parent.ContainsKey(currPath))
        {
            timer.WaitTime = pathTimestep;
            timer.Start();

            path.Add(currPath);

            if (currPath != rootIdx && currPath != dstIdx)
            {
                (container.GetChild(currPath) as ColorRect).SetFrameColor(pathColour);
            }
            parent.TryGetValue(currPath, out currPath);

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);
        (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);

        EmitSignal("finished");
        EmitSignal("finished_pathfinding", this, path, expanded);
    }

    public async void Dijkstra()
    {
        Godot.Collections.Array<int> expanded = new Godot.Collections.Array<int>();
        Godot.Collections.Array<int> path = new Godot.Collections.Array<int>();

        HashSet<int> closed = new HashSet<int>();

        HashSet<int> open = new HashSet<int>();
        open.Add(rootIdx);

        Dictionary<int, int> parent = new Dictionary<int, int>();

        Dictionary<int, int> gScores = new Dictionary<int, int>();
        for (int i = 0; i < width * height; i++)
        {
            gScores[i] = int.MaxValue;
        }
        gScores[rootIdx] = 0;

        Dictionary<int, int> fScores = new Dictionary<int, int>();
        for (int i = 0; i < width * height; i++)
        {
            fScores[i] = int.MaxValue;
        }
        fScores[rootIdx] = ManDist(rootIdx, dstIdx, width);

        while (open.Count > 0)
        {
            timer.WaitTime = searchTimestep;
            timer.Start();

            // you can use a priority queue here, looping because its easy
            int lowest = int.MaxValue;
            int curr = 0;
            foreach (int key in open)
            {
                if (fScores[key] < lowest)
                {
                    lowest = fScores[key];
                    curr = key;
                }
            }

            expanded.Add(curr);

            if (curr == dstIdx)
            {
                break;
            }

            if (curr != rootIdx)
            {
                (container.GetChild(curr) as ColorRect).SetFrameColor(expandedColour);
            }

            open.Remove(curr);
            closed.Add(curr);

            // queue the 4 directions
            foreach (int next in GetAdjacentNodes(curr, width, height))
            {
                if (!map[next])
                {
                    continue;
                }

                if (closed.Contains(next))
                {
                    continue;
                }

                // dist is always 1
                int gScore = gScores[curr] + 1;

                if (!open.Contains(next))
                {
                    open.Add(next);
                    if (next != rootIdx && next != dstIdx)
                    {
                        (container.GetChild(next) as ColorRect).SetFrameColor(searchColour);
                    }
                }

                if (gScore < gScores[next])
                {
                    parent[next] = curr;
                    gScores[next] = gScore ;
                    // dijkstra is basically just AStar with a huesteric function of zero
                    fScores[next] = gScores[next];
                }
            }

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        int currPath = dstIdx;
        while (parent.ContainsKey(currPath))
        {
            timer.WaitTime = pathTimestep;
            timer.Start();

            path.Add(currPath);

            if (currPath != rootIdx && currPath != dstIdx)
            {
                (container.GetChild(currPath) as ColorRect).SetFrameColor(pathColour);
            }
            parent.TryGetValue(currPath, out currPath);

            await ToSignal(timer, "timeout");
            timer.Stop();
        }

        (container.GetChild(rootIdx) as ColorRect).SetFrameColor(rootColour);
        (container.GetChild(dstIdx) as ColorRect).SetFrameColor(dstColour);

        EmitSignal("finished");
        EmitSignal("finished_pathfinding", this, path, expanded);
    }

    /// <summary>
    /// Adds the frontier cell of 2 width away from the side 
    /// </summary>
    /// <param name="frontierCells"></param>
    /// <param name="idx"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public List<int> GetFrontierCells(List<bool> map, int idx, int width, int height, bool state)
    {
        List<int> frontierCells = new List<int>();
        Vector2 loc = IdxToLoc(idx, width);

        for (int i = (int)loc.x - 2; i < (int)loc.x + 3; i = i + 2)
        {
            for (int j = (int)loc.y - 2; j < (int)loc.y + 3; j = j + 2)
            {   
                if ((i == loc.x && j != loc.y) || (i != loc.x && j == loc.y)) {
                    // same row or column
                    // out of bounds
                    if (i >= 0 && i < width && j >= 0 && j < height)
                    {
                        int frontIdx = j * width + i;        
                        // blockeds
                        if (map[frontIdx] == state)
                        {
                            frontierCells.Add(frontIdx);
                        } 
                    }
                }
            }
        }

        return frontierCells;
    }


    /// <summary>
    /// Returns whether there are L-squares based on an idx
    /// </summary>
    /// <param name="remainingCells"></param>
    /// <param name="idx"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public bool CheckLSquare(List<bool> map, int idx, int width, int height)
    {
        Vector2 loc = IdxToLoc(idx, width);
        // top left
        if (loc.x > 0 && loc.y > 0) {
            if (map[idx - width - 1] && map[idx - width] && map[idx - 1])
            {
                return false;
            }
        }
        // top right
        if (loc.x < width - 1 && loc.y > 0) {
            if (map[idx - width] && map[idx - width + 1] && map[idx + 1])
            {
                return false;
            }
        }
        // bottom left
        if (loc.x > 0 && loc.y < height - 1)
        {
            if (map[idx - 1] && map[idx + width - 1] && map[idx + width])
            {
                return false;
            }
        }
        // bottom right
        if (loc.x < width - 1 && loc.y < height - 1)
        {
            if (map[idx + 1] && map[idx + width] && map[idx + width + 1])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Adds the cells to the remaining ones
    /// </summary>
    /// <param name="remainingCells"></param>
    /// <param name="visitedMap"></param>
    /// <param name="idx"></param>
    /// <param name="width"></param>
    public void AddCells(List<int> remainingCells, List<bool> visitedMap, int idx, int width)
    {
        Vector2 loc = IdxToLoc(idx, width);

        if (loc.y > 0)
        {
            if (!visitedMap[idx - width] && !remainingCells.Contains(idx - width))
            {
                remainingCells.Add(idx - width);
            }
        }
        if (loc.x < width - 1)
        {
            if (!visitedMap[idx + 1] && !remainingCells.Contains(idx + 1))
            {
                remainingCells.Add(idx + 1);
            }
        }
        if (loc.y < height - 1)
        {
            if (!visitedMap[idx + width] && !remainingCells.Contains(idx + width))
            {
                remainingCells.Add(idx + width);
            }
        }
        if (loc.x > 0)
        {
            if (!visitedMap[idx - 1] && !remainingCells.Contains(idx - 1))
            {
                remainingCells.Add(idx - 1);
            }
        }
    }

    /// <summary>
    /// Coverst the index to location
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    public Vector2 IdxToLoc(int idx, int width)
    {
        return new Vector2(idx % width, (int)(idx / width));
    }

    public int ManDist(int src, int dst, int width)
    {
        Vector2 result = IdxToLoc(src, width) - IdxToLoc(dst, width);
        return (int)(Math.Abs(result.x) + Math.Abs(result.y));
    }

    /// <summary>
    /// Adds the walls based on the current cell
    /// </summary>
    /// <param name="remainingWalls"></param>
    /// <param name="visitedWalls"></param>
    /// <param name="loc"></param>
    public void AddWalls(List<int> remainingWalls, List<bool> visitedWalls, Vector2 loc)
    {
        int filler = (int)(loc.y * width + loc.x) * 4;

        if (loc.y > 0)
        {
            if (!visitedWalls[filler + 0]) {
                remainingWalls.Add(filler + 0);
            }
        }
        if (loc.x < width - 1)
        {
            if (!visitedWalls[filler + 1]) {
                remainingWalls.Add(filler + 1);
            }
        }
        if (loc.y < height - 1)
        {
            if (!visitedWalls[filler + 2]) {
                remainingWalls.Add(filler + 2);
            }
        }
        if (loc.x > 0)
        {
            if (!visitedWalls[filler + 3]) {
                remainingWalls.Add(filler + 3);
            }
        }
    }

    /// <summary>
    /// Returns the idx pair based on the wall and init loc
    /// </summary>
    /// <param name="nextWall"></param>
    /// <param name="side"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public Vector2 GetIdxPairFromWall(int nextWall, int side, int width, int height)
    {
        Vector2 pair = new Vector2();
        pair.x = (nextWall - side) / 4;
        pair.y = pair.x;

        if (side == 0)
        {
            pair.y -= width;
        }
        else if (side == 1)
        {
            pair.y += 1;
        }
        else if (side == 2)
        {
            pair.y += width;
        }
        else
        {
            pair.y -= 1;
        }

        return pair;
    }

    public List<int> GetAdjacentNodes(int idx, int width, int height)
    {
        List<int> adjacentNodes = new List<int>();

        Vector2 loc = IdxToLoc(idx, width);

        if (loc.y > 0)
        {
            adjacentNodes.Add(idx - width);
        }
        if (loc.x < width - 1)
        {
            adjacentNodes.Add(idx + 1);
        }
        if (loc.y < height - 1)
        {
            adjacentNodes.Add(idx + width);
        }
        if (loc.x > 0)
        {
            adjacentNodes.Add(idx - 1);
        }

        return adjacentNodes;
    }
}
