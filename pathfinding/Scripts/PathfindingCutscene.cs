using Godot;
using System;
using System.Collections.Generic;

public class PathfindingCutscene : CutScene
{
    Camera2D camera;
    Dictionary<NodeMap, bool> activeNodeMaps;
    Dictionary<NodeMap, Tuple<Godot.Collections.Array<int>, Godot.Collections.Array<int>>> paths;
    int iterations = 25;

    public void CheckActiveNodeMaps()
    {
        foreach (bool active in activeNodeMaps.Values)
        {
            if (active)
            {
                return;
            }
        }
        EmitSignal("active_nodemaps_finished");
    }

    public void OnPathFound(NodeMap map, Godot.Collections.Array<int> d, Godot.Collections.Array<int> e)
    {
        activeNodeMaps[map] = false;
        paths[map] = new Tuple<Godot.Collections.Array<int>, Godot.Collections.Array<int>>(d, e);

        // setting up maps
        GridContainer mapContainer = GetNode("SearchSimulation/NodeMaps") as GridContainer;
        List<NodeMap> maps = new List<NodeMap>();
        foreach (Node node in mapContainer.GetChildren())
        {
            maps.Add(node.GetNode("NodeMap") as NodeMap);
        }

        // setup the chart
        Control chart = GetNode("SearchSimulation/Chart") as Control;

        (chart.GetNode("Grid/DFSLabel2") as Label).Text = paths[maps[0]].Item1.Count.ToString();
        (chart.GetNode("Grid/DFSLabel3") as Label).Text = paths[maps[0]].Item2.Count.ToString();
        (chart.GetNode("Grid/BFSLabel2") as Label).Text = paths[maps[1]].Item1.Count.ToString();
        (chart.GetNode("Grid/BFSLabel3") as Label).Text = paths[maps[1]].Item2.Count.ToString();
        (chart.GetNode("Grid/DijkstraLabel2") as Label).Text = paths[maps[2]].Item1.Count.ToString();
        (chart.GetNode("Grid/DijkstraLabel3") as Label).Text = paths[maps[2]].Item2.Count.ToString();
        (chart.GetNode("Grid/AStarLabel2") as Label).Text = paths[maps[3]].Item1.Count.ToString();
        (chart.GetNode("Grid/AStarLabel3") as Label).Text = paths[maps[3]].Item2.Count.ToString();

        int place = maps.Count;
        foreach (bool active in activeNodeMaps.Values)
        {
            if (active)
            {
                place--;
            }
        }

        if (map == maps[0])
        {
            (chart.GetNode("Grid/DFSLabel4") as Label).Text = place.ToString();
        }
        else if(map == maps[1])
        {
            (chart.GetNode("Grid/BFSLabel4") as Label).Text = place.ToString();
        }
        else if (map == maps[2])
        {
            (chart.GetNode("Grid/DijkstraLabel4") as Label).Text = place.ToString();
        }
        else
        {
            (chart.GetNode("Grid/AStarLabel4") as Label).Text = place.ToString();
        }

        CheckActiveNodeMaps();
    }

    public async void WaitForActiveNodeMaps(NodeMap map)
    {
        await ToSignal(map, "finished");
        activeNodeMaps[map] = false;
        CheckActiveNodeMaps();
    }

    public override void _Ready()
    {
        camera = GetNode("Camera") as Camera2D;
        this.AddUserSignal("next");
        this.AddUserSignal("section_finished");

        activeNodeMaps = new Dictionary<NodeMap, bool>();
        paths = new Dictionary<NodeMap, Tuple<Godot.Collections.Array<int>, Godot.Collections.Array<int>>>();
        AddUserSignal("active_nodemaps_finished");

        Play();
    }

    /// <summary>
    /// Plays the selected cutscene.
    /// </summary>
    public async override void Play()
    {
        // all the animation tidbits are here
        //Intro();
        //await ToSignal(this, "section_finished");

        //Title();
        //await ToSignal(this, "section_finished");

        //Prologue();
        //await ToSignal(this, "section_finished");

        //SearchExplanation();
        //await ToSignal(this, "section_finished");

        SearchExample();
        await ToSignal(this, "section_finished");

        //SearchSimulation();
        //await ToSignal(this, "section_finished");

        //Summary();
        //await ToSignal(this, "section_finished");

        //Outro();
        //await ToSignal(this, "section_finished");

    }

    public async void Intro()
    {
        Control root = GetNode("Intro") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);
        /*
        Label label = root.GetNode("Title") as Label;
        await ToSignal(this, "next");
        label.SetVisible(true);
        tween = CommonTweenUtil.Fade(label, 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        */

        NodeMap maze = root.GetNode("NodeMap") as NodeMap;
        // set maze size
        maze.SetHeight(79);
        maze.SetWidth(159);
        maze.ResizeBlock();
        //maze.animate = false;

        await ToSignal(this, "next");
        maze.SetVisible(true);
        tween = CommonTweenUtil.Fade(maze, 0.3f, 0, 1, tween);
        //tween = CommonTweenUtil.Scale(maze, new Vector2(0, 0), new Vector2(1, 1), 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");
        maze.SetDst(159 * 2 - 2);

        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        tween.RemoveAll();
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }


    public async void Title()
    {
        Control root = GetNode("Title") as Control;
        root.SetVisible(true);

        Label label = GetNode("Title/Title") as Label;

        //Tween titleTween = CommonTweenUtil.ScaleCenter(label, new Vector2(0, 0), new Vector2(1, 1), 0.3f);
        await ToSignal(this, "next");
        label.SetVisible(true);
        Tween titleTween = CommonTweenUtil.Fade(label, 0.3f, 0, 1);
        AddChild(titleTween);
        titleTween.Start();

        await ToSignal(titleTween, "tween_completed");
        titleTween.QueueFree();

        await ToSignal(this, "next");
        titleTween = CommonTweenUtil.Fade(label, 0.3f, 1, 0);
        AddChild(titleTween);
        titleTween.Start();

        await ToSignal(titleTween, "tween_completed");
        titleTween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }

    public async void Prologue()
    {
        Control root = GetNode("Prologue") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);

        Legend legend = root.GetNode("Legend") as Legend;
        NodeMap maze = root.GetNode("NodeMap") as NodeMap;
        (legend.legendFigures.GetChild(0) as LegendFigure).SetIconColour(maze.wallColour);
        (legend.legendFigures.GetChild(1) as LegendFigure).SetIconColour(maze.mazeColour);
        (legend.legendFigures.GetChild(2) as LegendFigure).SetIconColour(maze.rootColour);
        (legend.legendFigures.GetChild(3) as LegendFigure).SetIconColour(maze.dstColour);
        (legend.legendFigures.GetChild(4) as LegendFigure).SetIconColour(maze.searchColour);
        (legend.legendFigures.GetChild(5) as LegendFigure).SetIconColour(maze.expandedColour);
        (legend.legendFigures.GetChild(6) as LegendFigure).SetIconColour(maze.pathColour);

        // set maze size
        maze.SetHeight(61);
        maze.SetWidth(121);
        maze.ResizeBlock();

        await ToSignal(this, "next");
        Control header = root.GetNode("Header") as Control;
        header.SetVisible(true);
        tween = CommonTweenUtil.Fade(header, 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.SetVisible(true);
        tween = CommonTweenUtil.Fade(maze, 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        legend.SetVisible(true);
        tween = CommonTweenUtil.Fade(legend, 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        (legend.legendFigures.GetChild(0) as LegendFigure).SetVisible(true);
        (legend.legendFigures.GetChild(1) as LegendFigure).SetVisible(true);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(0), 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(1), 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        (legend.legendFigures.GetChild(2) as LegendFigure).SetVisible(true);
        (legend.legendFigures.GetChild(3) as LegendFigure).SetVisible(true);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(2), 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(3), 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");
        maze.SetDst(121 * 2 - 2);

        await ToSignal(this, "next");
        (legend.legendFigures.GetChild(4) as LegendFigure).SetVisible(true);
        (legend.legendFigures.GetChild(5) as LegendFigure).SetVisible(true);
        (legend.legendFigures.GetChild(6) as LegendFigure).SetVisible(true);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(4), 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(5), 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.Fade(legend.legendFigures.GetChild(6), 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }

    public async void SearchExplanation()
    {
        Control root = GetNode("SearchExplanation") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);

        NodeMap maze = root.GetNode("NodeMap") as NodeMap;        
        maze.SetHeight(15);
        maze.SetWidth(15);
        maze.container.Set("custom_constants/vseparation", 1);
        maze.container.Set("custom_constants/hseparation", 1);
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                maze.SetCell(i * 15 + j, true);
            }
        }
        int dstIdx = 3 * 15 + 9;
        maze.SetDst(dstIdx);
        int rootIdx = 7 * 15 + 3;
        maze.SetRoot(rootIdx);

        Control header = root.GetNode("Header") as Control;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(header, 0.3f, 0, 1, tween);
        header.SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(maze, 0.3f, 0, 1, tween);
        maze.SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        Control description = root.GetNode("Description") as Control;
        description.SetVisible(true);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label1") as Control, 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        (description.GetNode("Label1") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        description = root.GetNode("Description2") as Control;
        description.SetVisible(true);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label3") as Control, 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        (description.GetNode("Label3") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label4") as Control, 0.3f, 0, 1, tween);
        (description.GetNode("Label4") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label5") as Control, 0.3f, 0, 1, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        (description.GetNode("Label5") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        description = root.GetNode("Description") as Control;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Move(root.GetNode("Description2") as Control, (root.GetNode("Description2") as Control).GetPosition() + new Vector2(100, 0), 0.3f, tween);
        tween = CommonTweenUtil.Fade(description.GetNode("Label2") as Control, 0.3f, 0, 1, tween);
        (description.GetNode("Label2") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();
        
        // simulate 1 more step
        rootIdx -= 15;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        // run AStar now but really slowly
        maze.searchTimestep = 0.1f;
        maze.pathTimestep = 0.1f;
        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        // now regenerate and show a full maze + astar
        await ToSignal(this, "next");
        for (int i  = 0; i < 15 * 15; i++)
        {
            tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i) as ColorRect), "color", maze.wallColour, 0.3f, tween);
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();
        maze.Clear();

        await ToSignal(this, "next");
        maze.useSeed = false;
        maze.GenerateMap();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }

    public async void SearchExample()
    {
        Control root = GetNode("SearchExample") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);

        Control mapContainers = root.GetNode("NodeMaps") as Control;
        List<NodeMap> maps = new List<NodeMap>();

        int seed = 89012634;
        foreach (Node map in mapContainers.GetChildren())
        {
            NodeMap temp = map.GetNode("NodeMap") as NodeMap;
            temp.SetHeight(61);
            temp.SetWidth(47);
            temp.ResizeBlock();
            temp.seed = seed;
            temp.useSeed = true;
            maps.Add(temp);
        }

        // rename the title and size
        (mapContainers.GetNode("DFS/GraphTitle") as Label).Text = "DFS - " + maps[0].width + " x " + maps[0].height;
        (mapContainers.GetNode("BFS/GraphTitle") as Label).Text = "BFS - " + maps[1].width + " x " + maps[1].height;
        (mapContainers.GetNode("Dijkstra/GraphTitle") as Label).Text = "Dijkstra - " + maps[2].width + " x " + maps[2].height;
        (mapContainers.GetNode("AStar/GraphTitle") as Label).Text = "AStar - " + maps[3].width + " x " + maps[3].height;

        await ToSignal(this, "next");
        Control header = root.GetNode("Header") as Control;
        header.SetVisible(true);
        tween = CommonTweenUtil.Fade(header, 0.3f, 0, 1, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        foreach (Node map in mapContainers.GetChildren())
        {
            tween = CommonTweenUtil.Fade(map, 0.3f, 0, 1, tween);
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        foreach (NodeMap map in maps)
        {
            map.seed = seed;
            map.GenerateMap();
        }
        foreach (NodeMap map in maps)
        {
            activeNodeMaps[map] = true;
            WaitForActiveNodeMaps(map);
        }
        await ToSignal(this, "active_nodemaps_finished");

        await ToSignal(this, "next");
        foreach (NodeMap map in maps)
        {
            activeNodeMaps[map] = true;
            WaitForActiveNodeMaps(map);
        }
        // record distance here and expansion
        maps[0].DFS();
        maps[1].BFS();
        maps[2].Dijkstra();
        maps[3].AStar();
        await ToSignal(this, "active_nodemaps_finished");

        await ToSignal(this, "next");
        for (int i = 1; i < mapContainers.GetChildCount(); i++)
        {
            tween = CommonTweenUtil.Fade(mapContainers.GetChild(i), 0.3f, 1, 0, tween);
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.MoveGlobal(mapContainers, new Vector2(1180, 303), 0.3f, tween, Tween.TransitionType.Quad, Tween.EaseType.In);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade((mapContainers.GetChild(0).GetChild(0) as Label), 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        NodeMap maze = maps[0];
        await ToSignal(this, "next");
        for (int i = 0; i < maze.width * maze.height; i++)
        {
            tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i) as ColorRect), "color", maze.wallColour, 0.3f, tween);
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetSize(mapContainers.GetChild(0).GetChild(1) as Control, new Vector2(630, 630), 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();
        maze.Clear();
     
        maze.container.Set("custom_constants/vseparation", 1);
        maze.container.Set("custom_constants/hseparation", 1);
        maze.SetHeight(15);
        maze.SetWidth(15);
        maze.ResizeBlock();

        await ToSignal(this, "next");
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        int dstIdx = 3 * 15 + 9;
        int rootIdx = 7 * 15 + 3;
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(dstIdx) as ColorRect), "color", maze.dstColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.rootColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                maze.SetCell(i * 15 + j, true);
            }
        }
        maze.SetDst(dstIdx);
        maze.SetRoot(rootIdx);

        // NOW DO DFS
        // simulate three steps here
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx -= 1;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx -= 1;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += 15;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        maze.searchTimestep = 0.1f;
        maze.pathTimestep = 0.1f;
        await ToSignal(this, "next");
        maze.DFS();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.DFS();
        await ToSignal(maze, "finished");

        maze.useSeed = false;

        // NOW DO BFS
        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        dstIdx = 3 * 15 + 9;
        rootIdx = 7 * 15 + 3;
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(dstIdx) as ColorRect), "color", maze.dstColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.rootColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                maze.SetCell(i * 15 + j, true);
            }
        }
        maze.SetDst(dstIdx);
        maze.SetRoot(rootIdx);

        // simulate four steps here
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx -= 15;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (1 + 15);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (-1 + 15);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (-15 - 1);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.BFS();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.BFS();
        await ToSignal(maze, "finished");

        // NOW DO DIJKSTRA
        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        dstIdx = 3 * 15 + 9;
        rootIdx = 7 * 15 + 3;
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(dstIdx) as ColorRect), "color", maze.dstColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.rootColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                maze.SetCell(i * 15 + j, true);
            }
        }
        maze.SetDst(dstIdx);
        maze.SetRoot(rootIdx);

        // simulate four steps here
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx -= 15;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (1 + 15);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (-1 + 15);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        rootIdx += (-15 - 1);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.expandedColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx - 1) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx + 15) as ColorRect), "color", maze.searchColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.Dijkstra();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.Dijkstra();
        await ToSignal(maze, "finished");

        // NOW DO ASTAR
        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        dstIdx = 3 * 15 + 9;
        rootIdx = 7 * 15 + 3;
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(dstIdx) as ColorRect), "color", maze.dstColour, 0.3f, tween);
        tween = CommonTweenUtil.SetProperty((maze.container.GetChild(rootIdx) as ColorRect), "color", maze.rootColour, 0.3f, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                maze.SetCell(i * 15 + j, true);
            }
        }
        maze.SetDst(dstIdx);
        maze.SetRoot(rootIdx);


        // simulate 2 steps here



        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.ResizeBlock();
        for (int i = 1; i < 15 - 1; i++)
        {
            for (int j = 1; j < 15 - 1; j++)
            {
                tween = CommonTweenUtil.SetProperty((maze.container.GetChild(i * 15 + j) as ColorRect), "color", maze.mazeColour, 0.3f, tween);
            }
        }
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        maze.GenerateMap();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        maze.AStar();
        await ToSignal(maze, "finished");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }

    public async void SearchSimulation()
    {
        Control root = GetNode("SearchSimulation") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);

        // setting up maps
        GridContainer mapContainer = root.GetNode("NodeMaps") as GridContainer;
        List<NodeMap> maps = new List<NodeMap>();
        foreach (Node node in mapContainer.GetChildren())
        {
            maps.Add(node.GetNode("NodeMap") as NodeMap);
        }

        // setup the chart
        Control chart = root.GetNode("Chart") as Control;


        Random random = new Random();
        int seed = -1;

        foreach (NodeMap maze in maps)
        {
            maze.SetWidth(55);
            maze.ResizeBlock();
        }

        // rename the title and size
        (mapContainer.GetNode("DFS/GraphTitle") as Label).Text = "DFS - " + maps[0].width + " x " + maps[0].height;
        (mapContainer.GetNode("BFS/GraphTitle") as Label).Text = "BFS - " + maps[1].width + " x " + maps[1].height;
        (mapContainer.GetNode("Dijkstra/GraphTitle") as Label).Text = "Dijkstra - " + maps[2].width + " x " + maps[2].height;
        (mapContainer.GetNode("AStar/GraphTitle") as Label).Text = "AStar - " + maps[3].width + " x " + maps[3].height;

        // reset the active nodemap
        activeNodeMaps.Clear();
        paths.Clear();
        foreach (NodeMap map in maps)
        {
            activeNodeMaps.Add(map, true);
            // connect the signals
            map.Connect("finished_pathfinding", this, nameof(OnPathFound));
            paths[map] = new Tuple<Godot.Collections.Array<int>, Godot.Collections.Array<int>>(new Godot.Collections.Array<int>(), new Godot.Collections.Array<int>());
        }

        for (int i = 0; i < 20; i++) {
            seed = random.Next();
 
            foreach (NodeMap maze in maps)
            {
                maze.seed = seed;
                maze.GenerateMap();
            }

            foreach (NodeMap map in maps)
            {
                activeNodeMaps[map] = true;
                WaitForActiveNodeMaps(map);
            }

            await ToSignal(this, "active_nodemaps_finished");

            // record place
            foreach (NodeMap map in maps)
            {
                activeNodeMaps[map] = true;
            }

            // record distance here and expansion
            maps[0].DFS();
            maps[1].BFS();
            maps[2].Dijkstra();
            maps[3].AStar();

            await ToSignal(this, "active_nodemaps_finished");

            // toggle all the info now
        }

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    
    }

    public async void Summary()
    {
        Control root = GetNode("Summary") as Control;
        root.SetVisible(true);

        Tween tween = new Tween();
        AddChild(tween);

        Control header = root.GetNode("Header") as Control;
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(header, 0.3f, 0, 1, tween);
        header.SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        Control description = root.GetNode("Description") as Control;
        description.SetVisible(true);
        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label1") as Control, 0.3f, 0, 1, tween);
        (description.GetNode("Label1") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label2") as Control, 0.3f, 0, 1, tween);
        (description.GetNode("Label2") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(description.GetNode("Label3") as Control, 0.3f, 0, 1, tween);
        (description.GetNode("Label3") as Control).SetVisible(true);
        tween.Start();
        await ToSignal(tween, "tween_completed");

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.QueueFree();
        root.SetVisible(false);

        EmitSignal("section_finished");
    }

    public async void Outro()
    {
        Control root = GetNode("Outro") as Control;
        root.SetVisible(true);

        Label label = root.GetNode("Title") as Label;

        await ToSignal(this, "next");
        Tween tween = CommonTweenUtil.Fade(label, 0.3f, 0, 1);
        label.SetVisible(true);
        AddChild(tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();

        await ToSignal(this, "next");
        tween = CommonTweenUtil.Fade(root, 0.3f, 1, 0, tween);
        tween.Start();
        await ToSignal(tween, "tween_completed");
        tween.RemoveAll();
        EmitSignal("section_finished");
        root.SetVisible(false);
    }
}
