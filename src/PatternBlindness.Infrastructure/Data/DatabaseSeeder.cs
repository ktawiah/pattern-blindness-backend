using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PatternBlindness.Domain.Entities;
using PatternBlindness.Domain.Enums;

namespace PatternBlindness.Infrastructure.Data;

public static class DatabaseSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Always update patterns to ensure they have comprehensive data
        await SeedPatternsAsync(context);
        await SeedDataStructuresAsync(context);
    }

    private static async Task SeedPatternsAsync(ApplicationDbContext context)
    {
        var patterns = CreatePatterns();

        foreach (var newPattern in patterns)
        {
            var existingPattern = await context.Patterns
                .FirstOrDefaultAsync(p => p.Name == newPattern.Name);

            if (existingPattern == null)
            {
                await context.Patterns.AddAsync(newPattern);
            }
            else
            {
                // Update existing pattern with new comprehensive data using Update method
                existingPattern.Update(
                    name: newPattern.Name,
                    description: newPattern.Description,
                    whatItIs: newPattern.WhatItIs,
                    whenToUse: newPattern.WhenToUse,
                    whyItWorks: newPattern.WhyItWorks,
                    commonUseCases: newPattern.CommonUseCases,
                    timeComplexity: newPattern.TimeComplexity,
                    spaceComplexity: newPattern.SpaceComplexity,
                    pseudoCode: newPattern.PseudoCode,
                    triggerSignals: newPattern.TriggerSignals,
                    commonMistakes: newPattern.CommonMistakes,
                    resources: newPattern.Resources,
                    relatedPatternIds: newPattern.RelatedPatternIds
                );
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDataStructuresAsync(ApplicationDbContext context)
    {
        var dataStructures = CreateDataStructures();

        foreach (var newDs in dataStructures)
        {
            var existingDs = await context.DataStructures
                .FirstOrDefaultAsync(ds => ds.Name == newDs.Name);

            if (existingDs == null)
            {
                await context.DataStructures.AddAsync(newDs);
            }
            else
            {
                existingDs.Update(
                    name: newDs.Name,
                    description: newDs.Description,
                    whatItIs: newDs.WhatItIs,
                    operations: newDs.Operations,
                    whenToUse: newDs.WhenToUse,
                    tradeoffs: newDs.Tradeoffs,
                    commonUseCases: newDs.CommonUseCases,
                    implementation: newDs.Implementation,
                    resources: newDs.Resources,
                    relatedStructureIds: newDs.RelatedStructureIds
                );
            }
        }

        await context.SaveChangesAsync();
    }

    private static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj, JsonOptions);

    private static List<Pattern> CreatePatterns()
    {
        return
        [
            // Two Pointers Pattern
            Pattern.Create(
                name: "Two Pointers",
                description: "Use two pointers to traverse a data structure, typically from opposite ends or at different speeds.",
                category: PatternCategory.TwoPointers,
                whatItIs: @"Two Pointers is a technique where you use two references (pointers) to traverse a data structure. The pointers can:
- Start from opposite ends and move toward each other
- Move at different speeds (slow/fast pointer)
- Both start from the same end moving in the same direction

This technique reduces the need for nested loops, often improving time complexity from O(n²) to O(n).",
                whenToUse: @"Use Two Pointers when:
- The input is sorted or has some ordering property
- You need to find pairs that satisfy a condition
- You need to compare elements from different positions
- Working with palindromes or symmetric structures
- Detecting cycles in linked lists",
                whyItWorks: @"Two Pointers works because:
1. Sorted data allows making decisions based on comparisons
2. Moving pointers based on conditions eliminates unnecessary comparisons
3. The approach leverages the structure of the data to skip invalid combinations",
                commonUseCases: ToJson(new[] { "Two Sum in sorted array", "Container with most water", "Palindrome check", "Remove duplicates", "Merge sorted arrays" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(1)",
                pseudoCode: @"TWO POINTERS ALGORITHM (Opposite Direction)

1. INITIALIZE two pointers:
   - left pointer at the start of the array (index 0)
   - right pointer at the end of the array (index n-1)

2. WHILE left < right:
   a. EXAMINE elements at both pointers
   b. IF the pair satisfies the target condition:
      - Record or return the result
   c. DECIDE which pointer to move:
      - Move LEFT pointer right if you need a larger value at left
      - Move RIGHT pointer left if you need a smaller value at right
   d. INCREMENT/DECREMENT the chosen pointer

3. RETURN result (or indicate no valid pair found)

KEY INSIGHT: In a sorted array, moving the left pointer increases the value,
moving the right pointer decreases it. This allows systematic elimination
of invalid pairs without checking all O(n²) combinations.",
                triggerSignals: ToJson(new[] { "Sorted array/string", "Find pair with sum/difference", "Palindrome", "In-place modification", "Comparing from both ends" }),
                commonMistakes: ToJson(new[] { "Off-by-one errors with indices", "Not handling duplicates properly", "Wrong pointer movement direction", "Missing edge cases (empty array, single element)" }),
                resources: ToJson(new object[] {
                    new { title = "Two Pointers - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/4", type = "course" },
                    new { title = "Two Pointers Pattern - LeetCode", url = "https://leetcode.com/explore/interview/card/leetcodes-interview-crash-course-data-structures-and-algorithms/703/arraystrings/4501/", type = "article" },
                    new { title = "Two Pointers - TakeUForward", url = "https://takeuforward.org/data-structure/two-pointers-technique/", type = "article" },
                    new { title = "Two Pointers Tutorial - GeeksforGeeks", url = "https://www.geeksforgeeks.org/two-pointers-technique/", type = "article" },
                    new { title = "Two Pointers Problems - LeetCode", url = "https://leetcode.com/tag/two-pointers/", type = "practice" },
                    new { title = "Two Pointers Explained - Abdul Bari", url = "https://www.youtube.com/watch?v=On03HWe2tZM", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Sliding Window Pattern
            Pattern.Create(
                name: "Sliding Window",
                description: "Maintain a window of elements that slides through the data structure to find optimal subarrays/substrings.",
                category: PatternCategory.SlidingWindow,
                whatItIs: @"Sliding Window maintains a contiguous subset (window) of elements and slides it through the input. Two types:
- Fixed size: Window size stays constant
- Variable size: Window expands/contracts based on conditions

The window typically tracks a running state (sum, count, set of characters) that updates incrementally.",
                whenToUse: @"Use Sliding Window when:
- Looking for subarrays/substrings satisfying a condition
- Need to find min/max length subarray with some property
- Working with contiguous sequences
- The problem mentions 'consecutive' or 'contiguous'",
                whyItWorks: @"Sliding Window works because:
1. Incrementally updating window state is O(1) vs recomputing from scratch
2. The window maintains invariants that help identify valid solutions
3. Expansion/contraction logic systematically explores all possibilities",
                commonUseCases: ToJson(new[] { "Longest substring without repeats", "Minimum window substring", "Maximum sum subarray of size k", "Find all anagrams", "Longest subarray with sum ≤ k" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(k) where k is window size or character set",
                pseudoCode: @"SLIDING WINDOW ALGORITHM (Variable Size)

1. INITIALIZE:
   - left pointer = 0
   - window_state = empty (could be sum, count, hash map, etc.)
   - result = initial value (0, infinity, empty, depending on min/max)

2. FOR each position (right) from 0 to n-1:
   a. EXPAND: Add element at right to window_state

   b. SHRINK: While the window is invalid (violates constraint):
      - Remove element at left from window_state
      - Move left pointer forward (left++)

   c. UPDATE: If current window is valid:
      - Update result if this window is better (longer/shorter/etc.)

3. RETURN result

KEY INSIGHT: Each element enters the window once (when right moves)
and leaves once (when left moves), giving O(n) total operations.

FOR FIXED SIZE K:
- Same structure, but shrink when window size exceeds k
- Condition becomes: (right - left + 1) > k",
                triggerSignals: ToJson(new[] { "Contiguous subarray/substring", "Maximum/minimum length", "Fixed window size k", "Substring containing all characters", "At most k distinct" }),
                commonMistakes: ToJson(new[] { "Forgetting to shrink window when needed", "Not handling empty window case", "Off-by-one in window size calculation", "Incorrect state update when elements leave window" }),
                resources: ToJson(new object[] {
                    new { title = "Sliding Window - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/1", type = "course" },
                    new { title = "Sliding Window for Beginners", url = "https://leetcode.com/discuss/general-discussion/657507/sliding-window-for-beginners-problems-template-sample-solutions", type = "article" },
                    new { title = "Sliding Window - TakeUForward", url = "https://takeuforward.org/data-structure/sliding-window-technique/", type = "article" },
                    new { title = "Sliding Window - GeeksforGeeks", url = "https://www.geeksforgeeks.org/window-sliding-technique/", type = "article" },
                    new { title = "Sliding Window Problems - LeetCode", url = "https://leetcode.com/tag/sliding-window/", type = "practice" },
                    new { title = "Sliding Window - Aditya Verma", url = "https://www.youtube.com/playlist?list=PL_z_8CaSLPWeM8BDJmIYDnRPQgNPnGvJv", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Binary Search Pattern
            Pattern.Create(
                name: "Binary Search",
                description: "Repeatedly divide the search space in half to find target or optimal value in logarithmic time.",
                category: PatternCategory.BinarySearch,
                whatItIs: @"Binary Search divides a sorted space in half with each comparison. Beyond finding exact values, it can:
- Find insertion position
- Search for first/last occurrence
- Search on answer (binary search the solution space)

Key insight: If the search space has a monotonic property, binary search applies.",
                whenToUse: @"Use Binary Search when:
- Input is sorted or has monotonic property
- Need to find exact value, boundary, or optimal answer
- Search space can be clearly defined with low/high bounds
- A function f(x) transitions from false to true (or vice versa)",
                whyItWorks: @"Binary Search works because:
1. Each comparison eliminates half the remaining search space
2. Monotonicity ensures the answer lies in one half after each check
3. Log₂(n) halvings are needed to reach a single element",
                commonUseCases: ToJson(new[] { "Search in sorted array", "Find first/last position", "Search in rotated array", "Koko eating bananas", "Capacity to ship packages", "Square root" }),
                timeComplexity: "O(log n)",
                spaceComplexity: "O(1) iterative, O(log n) recursive",
                pseudoCode: @"BINARY SEARCH ALGORITHM

1. INITIALIZE:
   - left = 0 (first index)
   - right = n - 1 (last index)

2. WHILE left ≤ right:
   a. CALCULATE middle index:
      mid = left + (right - left) / 2
      [Note: This avoids integer overflow vs (left + right) / 2]

   b. COMPARE element at mid with target:
      - IF arr[mid] == target: Found! Return mid
      - IF arr[mid] < target: Search right half (left = mid + 1)
      - IF arr[mid] > target: Search left half (right = mid - 1)

3. RETURN -1 (not found) or left (insertion point)

VARIATIONS:

Find First Occurrence:
- When arr[mid] == target, set right = mid - 1 and continue
- Return left if arr[left] == target

Find Last Occurrence:
- When arr[mid] == target, set left = mid + 1 and continue
- Return right if arr[right] == target

Binary Search on Answer Space:
- Define condition function: canAchieve(value)
- Search for minimum/maximum value where condition is true",
                triggerSignals: ToJson(new[] { "Sorted array", "Find minimum/maximum that satisfies condition", "Search space with clear bounds", "Minimize maximum or maximize minimum", "O(log n) requirement" }),
                commonMistakes: ToJson(new[] { "Integer overflow in mid calculation", "Wrong boundary update (mid vs mid±1)", "Incorrect loop condition (< vs <=)", "Not handling empty array", "Off-by-one in result" }),
                resources: ToJson(new object[] {
                    new { title = "Binary Search - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/2", type = "course" },
                    new { title = "Binary Search 101", url = "https://leetcode.com/problems/binary-search/solutions/423162/Binary-Search-101/", type = "article" },
                    new { title = "Binary Search - TakeUForward", url = "https://takeuforward.org/data-structure/binary-search-explained/", type = "article" },
                    new { title = "Binary Search - GeeksforGeeks", url = "https://www.geeksforgeeks.org/binary-search/", type = "article" },
                    new { title = "Binary Search Problems - LeetCode", url = "https://leetcode.com/tag/binary-search/", type = "practice" },
                    new { title = "Binary Search - Errichto", url = "https://www.youtube.com/watch?v=GU7DpgHINWQ", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // DFS Pattern
            Pattern.Create(
                name: "Depth-First Search (DFS)",
                description: "Explore as far as possible along each branch before backtracking, used for trees, graphs, and recursive exploration.",
                category: PatternCategory.Graph,
                whatItIs: @"DFS explores a graph or tree by going as deep as possible before backtracking. It uses:
- Recursion (implicit stack) or explicit stack
- Visited set to avoid cycles in graphs
- Can be preorder, inorder, or postorder for trees",
                whenToUse: @"Use DFS when:
- Need to explore all paths or possibilities
- Finding connected components
- Detecting cycles
- Path finding where path matters (not just reachability)
- Tree traversals
- Backtracking problems",
                whyItWorks: @"DFS works because:
1. The stack (implicit or explicit) naturally handles the backtracking
2. Systematic exploration ensures all reachable nodes are visited
3. Memory efficient for deep graphs (only stores current path)",
                commonUseCases: ToJson(new[] { "Number of islands", "Path sum in tree", "Clone graph", "Course schedule", "Word search", "Generate permutations" }),
                timeComplexity: "O(V + E) for graphs, O(n) for trees",
                spaceComplexity: "O(h) where h is height/depth",
                pseudoCode: @"DEPTH-FIRST SEARCH (DFS) ALGORITHM

RECURSIVE VERSION:
1. BASE CASE: If node is null or already visited, return
2. Mark current node as visited
3. Process current node (depends on problem)
4. FOR each neighbor of current node:
   - Recursively call DFS on neighbor

ITERATIVE VERSION (using explicit stack):
1. INITIALIZE:
   - Push start node onto stack
   - Create empty visited set

2. WHILE stack is not empty:
   a. Pop node from stack
   b. IF node already visited, skip it
   c. Mark node as visited
   d. Process node
   e. Push all unvisited neighbors onto stack

TREE TRAVERSAL ORDERS:
- Preorder: Process node BEFORE children
- Inorder: Process node BETWEEN children (binary trees)
- Postorder: Process node AFTER children

KEY INSIGHT: DFS naturally handles backtracking - when recursion
returns (or we pop from stack), we 'backtrack' to previous state.",
                triggerSignals: ToJson(new[] { "Explore all paths", "Connected components", "Tree traversal", "Detect cycle", "Path exists", "Generate combinations" }),
                commonMistakes: ToJson(new[] { "Forgetting to mark visited", "Not handling cycles in graphs", "Wrong base case", "Stack overflow on deep recursion", "Modifying collection while iterating" }),
                resources: ToJson(new object[] {
                    new { title = "Graph DFS - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/7", type = "course" },
                    new { title = "DFS - TakeUForward", url = "https://takeuforward.org/data-structure/depth-first-search-dfs/", type = "article" },
                    new { title = "DFS - GeeksforGeeks", url = "https://www.geeksforgeeks.org/depth-first-search-or-dfs-for-a-graph/", type = "article" },
                    new { title = "DFS Problems - LeetCode", url = "https://leetcode.com/tag/depth-first-search/", type = "practice" },
                    new { title = "DFS Tutorial - William Fiset", url = "https://www.youtube.com/watch?v=7fujbpJ0LB4", type = "video" },
                    new { title = "DFS Visualizer - VisuAlgo", url = "https://visualgo.net/en/dfsbfs", type = "visualization" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // BFS Pattern
            Pattern.Create(
                name: "Breadth-First Search (BFS)",
                description: "Explore all neighbors at present depth before moving to nodes at next depth level, ideal for shortest paths.",
                category: PatternCategory.Graph,
                whatItIs: @"BFS explores a graph level by level using a queue. Key properties:
- First path found to any node is the shortest (in unweighted graphs)
- Processes all nodes at distance d before distance d+1
- Can be used for level-order traversal of trees",
                whenToUse: @"Use BFS when:
- Finding shortest path in unweighted graph
- Level-order traversal needed
- Finding minimum steps/moves
- Multi-source shortest path
- Problems involving 'nearest' or 'minimum distance'",
                whyItWorks: @"BFS works because:
1. The queue ensures FIFO processing (first discovered = first processed)
2. Level-by-level exploration guarantees shortest path in unweighted graphs
3. All nodes at distance d are processed before distance d+1",
                commonUseCases: ToJson(new[] { "Shortest path in maze", "Level order traversal", "Rotting oranges", "Word ladder", "Minimum knight moves", "Nearest exit in maze" }),
                timeComplexity: "O(V + E) for graphs, O(n) for trees",
                spaceComplexity: "O(V) for the queue",
                pseudoCode: @"BREADTH-FIRST SEARCH (BFS) ALGORITHM

1. INITIALIZE:
   - Create a queue and add the start node
   - Create a visited set and mark start as visited
   - Set level/distance = 0

2. WHILE queue is not empty:
   a. Get the number of nodes at current level (queue size)

   b. FOR each node at current level:
      - Remove node from front of queue
      - Process node (check if goal, record result, etc.)
      - FOR each neighbor of node:
        - IF neighbor not visited:
          - Mark neighbor as visited
          - Add neighbor to back of queue

   c. INCREMENT level/distance counter

3. RETURN result (path length, reachable nodes, etc.)

MULTI-SOURCE BFS:
- Start by adding ALL source nodes to queue
- Useful for problems like 'rotting oranges' or 'walls and gates'

KEY INSIGHT: BFS guarantees shortest path in unweighted graphs
because it explores all nodes at distance d before distance d+1.",
                triggerSignals: ToJson(new[] { "Shortest path unweighted", "Minimum steps/moves", "Level-order traversal", "Nearest/closest", "Spreading/infection simulation" }),
                commonMistakes: ToJson(new[] { "Using DFS for shortest path", "Not tracking visited before enqueueing", "Processing level boundaries incorrectly", "Forgetting to handle disconnected components" }),
                resources: ToJson(new object[] {
                    new { title = "Graph BFS - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/8", type = "course" },
                    new { title = "BFS - TakeUForward", url = "https://takeuforward.org/data-structure/breadth-first-search-bfs-level-order-traversal/", type = "article" },
                    new { title = "BFS - GeeksforGeeks", url = "https://www.geeksforgeeks.org/breadth-first-search-or-bfs-for-a-graph/", type = "article" },
                    new { title = "BFS Problems - LeetCode", url = "https://leetcode.com/tag/breadth-first-search/", type = "practice" },
                    new { title = "BFS Tutorial - William Fiset", url = "https://www.youtube.com/watch?v=oDqjPvD54Ss", type = "video" },
                    new { title = "BFS Visualizer - VisuAlgo", url = "https://visualgo.net/en/dfsbfs", type = "visualization" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Dynamic Programming Pattern
            Pattern.Create(
                name: "Dynamic Programming",
                description: "Break down problems into overlapping subproblems and store solutions to avoid redundant computation.",
                category: PatternCategory.DynamicProgramming,
                whatItIs: @"Dynamic Programming (DP) solves problems by:
1. Breaking them into smaller overlapping subproblems
2. Solving each subproblem once and storing the result
3. Building up to the final solution

Two approaches: Top-down (memoization) and Bottom-up (tabulation).",
                whenToUse: @"Use DP when:
- Problem has optimal substructure (optimal solution contains optimal solutions to subproblems)
- Problem has overlapping subproblems (same subproblems solved multiple times)
- Need to count ways, find min/max, or make optimal decisions",
                whyItWorks: @"DP works because:
1. Storing solutions eliminates redundant computation
2. Optimal substructure means we can build optimal solution from subproblem solutions
3. Systematic approach ensures all possibilities are considered",
                commonUseCases: ToJson(new[] { "Fibonacci", "Climbing stairs", "House robber", "Longest common subsequence", "Knapsack", "Edit distance", "Coin change" }),
                timeComplexity: "Varies: O(n), O(n²), O(n×m), etc.",
                spaceComplexity: "O(n) to O(n×m), often reducible",
                pseudoCode: @"DYNAMIC PROGRAMMING ALGORITHM

STEP 1: Define the State
- What does dp[i] or dp[i][j] represent?
- State should capture all information needed for decisions

STEP 2: Identify Base Cases
- What are the smallest subproblems with known answers?
- Examples: dp[0] = 0, dp[1] = 1, empty string = 0

STEP 3: Find the Recurrence Relation
- How does dp[i] relate to smaller subproblems?
- Consider all possible last choices/transitions

STEP 4: Determine Computation Order
- Bottom-up: Compute smaller subproblems first
- Top-down: Use memoization with recursion

BOTTOM-UP APPROACH:
1. Create dp array of appropriate size
2. Initialize base cases
3. FOR i from (base case + 1) to n:
   - Compute dp[i] using recurrence relation
4. Return dp[n]

TOP-DOWN APPROACH (Memoization):
1. Create memo dictionary/array
2. Define recursive function with memo lookup
3. IF state in memo, return cached result
4. Compute result recursively
5. Store in memo before returning

SPACE OPTIMIZATION: If dp[i] only depends on previous k states,
use rolling array of size k instead of full array.",
                triggerSignals: ToJson(new[] { "Count number of ways", "Minimum/maximum value", "Can you reach target?", "Longest/shortest sequence", "Make choices at each step", "Optimal decision sequence" }),
                commonMistakes: ToJson(new[] { "Wrong base case", "Incorrect recurrence relation", "Not considering all transitions", "Index out of bounds", "Not optimizing space when possible" }),
                resources: ToJson(new object[] {
                    new { title = "DP - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/5", type = "course" },
                    new { title = "DP Patterns", url = "https://leetcode.com/discuss/general-discussion/458695/dynamic-programming-patterns", type = "article" },
                    new { title = "DP - TakeUForward", url = "https://takeuforward.org/dynamic-programming/striver-dp-series-dynamic-programming-problems/", type = "article" },
                    new { title = "DP Playlist - Aditya Verma", url = "https://www.youtube.com/playlist?list=PL_z_8CaSLPWekqhdCPmFohncHwz8TY2Go", type = "video" },
                    new { title = "DP Problems - LeetCode", url = "https://leetcode.com/tag/dynamic-programming/", type = "practice" },
                    new { title = "DP - GeeksforGeeks", url = "https://www.geeksforgeeks.org/dynamic-programming/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Backtracking Pattern
            Pattern.Create(
                name: "Backtracking",
                description: "Build solutions incrementally, abandoning (backtracking) paths that fail to satisfy constraints.",
                category: PatternCategory.Backtracking,
                whatItIs: @"Backtracking is a systematic way to explore all possible solutions by:
1. Making a choice
2. Recursively exploring with that choice
3. Undoing the choice (backtrack) and trying alternatives

It's essentially DFS on the decision tree with pruning.",
                whenToUse: @"Use Backtracking when:
- Need to generate all possible solutions
- Problem involves making sequences of decisions
- Need to find solutions satisfying constraints
- Exploring permutations, combinations, or subsets",
                whyItWorks: @"Backtracking works because:
1. Systematic exploration ensures completeness
2. Early pruning avoids exploring invalid paths
3. The undo step allows reusing state for different choices",
                commonUseCases: ToJson(new[] { "N-Queens", "Sudoku solver", "Generate permutations", "Combination sum", "Word search", "Palindrome partitioning" }),
                timeComplexity: "Often O(n!) or O(2^n) - exponential",
                spaceComplexity: "O(n) for recursion depth",
                pseudoCode: @"BACKTRACKING ALGORITHM

1. DEFINE the recursive backtrack function:
   backtrack(current_state, choices_remaining)

2. BASE CASE - Check if current state is a complete solution:
   - IF is_solution(current_state):
     - Add copy of current_state to results
     - Return

3. FOR each choice in available choices:
   a. CHECK if choice is valid (pruning):
      - Skip invalid choices early to save time

   b. MAKE the choice:
      - Add choice to current_state
      - Update any tracking variables

   c. RECURSE:
      - Call backtrack with updated state

   d. UNDO the choice (backtrack):
      - Remove choice from current_state
      - Restore tracking variables to previous values

KEY INSIGHT: The 'undo' step is what makes it backtracking.
We explore one path completely, then backtrack to try alternatives.

PRUNING STRATEGIES:
- Skip choices that violate constraints
- Skip choices that can't lead to better solutions
- Use sorting to enable early termination",
                triggerSignals: ToJson(new[] { "Generate all permutations/combinations", "Find all solutions", "Constraint satisfaction", "Explore decision tree", "Can't use DP (no overlapping subproblems)" }),
                commonMistakes: ToJson(new[] { "Forgetting to backtrack (undo choice)", "Not copying path when saving solution", "Inefficient pruning", "Wrong base case", "Duplicate solutions due to order" }),
                resources: ToJson(new object[] {
                    new { title = "Backtracking - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/6", type = "course" },
                    new { title = "Backtracking - TakeUForward", url = "https://takeuforward.org/data-structure/recursion-and-backtracking/", type = "article" },
                    new { title = "Backtracking - GeeksforGeeks", url = "https://www.geeksforgeeks.org/backtracking-algorithms/", type = "article" },
                    new { title = "Backtracking Problems - LeetCode", url = "https://leetcode.com/tag/backtracking/", type = "practice" },
                    new { title = "Recursion & Backtracking - Abdul Bari", url = "https://www.youtube.com/watch?v=DKCbsiDBN6c", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Heap/Priority Queue Pattern
            Pattern.Create(
                name: "Heap / Priority Queue",
                description: "Efficiently maintain and access the minimum or maximum element in a dynamic collection.",
                category: PatternCategory.Heap,
                whatItIs: @"A Heap is a complete binary tree where each parent is smaller (min-heap) or larger (max-heap) than its children. Operations:
- Insert: O(log n)
- Extract min/max: O(log n)
- Peek min/max: O(1)

Python's heapq is a min-heap; negate values for max-heap behavior.",
                whenToUse: @"Use Heap when:
- Need repeated access to min/max element
- Finding k largest/smallest elements
- Merge k sorted lists/streams
- Scheduling problems (process smallest/largest next)
- Median maintenance",
                whyItWorks: @"Heaps work because:
1. The heap property ensures min/max is always at root
2. Tree structure gives O(log n) insert and extract
3. Efficient for problems needing repeated min/max access",
                commonUseCases: ToJson(new[] { "Kth largest element", "Merge k sorted lists", "Find median from stream", "Task scheduler", "Top k frequent elements", "Meeting rooms" }),
                timeComplexity: "O(log n) per operation, O(n log k) for k elements",
                spaceComplexity: "O(n) or O(k)",
                pseudoCode: @"HEAP / PRIORITY QUEUE ALGORITHM

HEAP OPERATIONS:
- INSERT: Add element, bubble up to maintain heap property - O(log n)
- EXTRACT-MIN/MAX: Remove root, replace with last, bubble down - O(log n)
- PEEK: Return root element without removing - O(1)
- HEAPIFY: Convert array to heap - O(n)

MIN-HEAP PROPERTY: Parent ≤ both children
MAX-HEAP PROPERTY: Parent ≥ both children

COMMON PATTERNS:

1. K-th Largest/Smallest:
   - Use min-heap of size k for k-th largest
   - Use max-heap of size k for k-th smallest
   - Maintain heap size; root is the answer

2. Merge K Sorted Lists:
   - Add first element of each list to heap
   - Extract minimum, add next element from that list
   - Repeat until heap is empty

3. Top K Frequent:
   - Count frequencies
   - Use heap to get top k by frequency

4. Two Heaps for Median:
   - Max-heap for lower half
   - Min-heap for upper half
   - Balance sizes; median is at/between roots",
                triggerSignals: ToJson(new[] { "Kth largest/smallest", "Merge k sorted", "Continuously find min/max", "Top k elements", "Schedule by priority", "Median maintenance" }),
                commonMistakes: ToJson(new[] { "Using wrong heap type (min vs max)", "Forgetting to negate for max-heap in Python", "Not handling equal elements correctly", "Inefficient: sorting when heap suffices" }),
                resources: ToJson(new object[] {
                    new { title = "Heaps - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/3", type = "course" },
                    new { title = "Heaps - TakeUForward", url = "https://takeuforward.org/data-structure/heap-priority-queue/", type = "article" },
                    new { title = "Heaps - GeeksforGeeks", url = "https://www.geeksforgeeks.org/heap-data-structure/", type = "article" },
                    new { title = "Heap Problems - LeetCode", url = "https://leetcode.com/tag/heap-priority-queue/", type = "practice" },
                    new { title = "Heaps - William Fiset", url = "https://www.youtube.com/watch?v=t0Cq6tVNRBA", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Greedy Pattern
            Pattern.Create(
                name: "Greedy",
                description: "Make locally optimal choices at each step, hoping to find a global optimum.",
                category: PatternCategory.Greedy,
                whatItIs: @"Greedy algorithms make the best choice at each step without considering future consequences. Key requirements:
1. Greedy choice property: local optimal leads to global optimal
2. Optimal substructure: optimal solution contains optimal sub-solutions

Not all problems have greedy solutions - proof or intuition needed.",
                whenToUse: @"Use Greedy when:
- Problem has greedy choice property
- Making locally optimal choice doesn't prevent global optimum
- Interval scheduling problems
- Huffman coding, MST algorithms
- Problems where sorting helps make decisions",
                whyItWorks: @"Greedy works because:
1. For certain problems, the locally optimal choice is part of the globally optimal solution
2. No need to explore all possibilities - one path suffices
3. Usually leads to efficient O(n log n) or O(n) solutions",
                commonUseCases: ToJson(new[] { "Activity selection", "Jump game", "Gas station", "Assign cookies", "Non-overlapping intervals", "Partition labels" }),
                timeComplexity: "Usually O(n log n) due to sorting, or O(n)",
                spaceComplexity: "Usually O(1) to O(n)",
                pseudoCode: @"GREEDY ALGORITHM

GENERAL FRAMEWORK:
1. IDENTIFY the greedy choice property:
   - What locally optimal choice leads to global optimum?

2. SORT (often required):
   - By start time, end time, ratio, value, etc.
   - Sorting order depends on the greedy strategy

3. ITERATE and make greedy choices:
   FOR each item (in sorted order):
      IF item can be included (doesn't violate constraints):
         Include item in solution
         Update current state

4. RETURN result

PROVING GREEDY WORKS:
- Greedy Choice Property: Local optimum leads to global optimum
- Optimal Substructure: Optimal solution contains optimal sub-solutions
- Exchange Argument: Show any other choice can be 'exchanged' without loss

COMMON GREEDY STRATEGIES:
- Activity Selection: Sort by end time, pick non-overlapping
- Fractional Knapsack: Sort by value/weight ratio
- Huffman Coding: Always merge two smallest frequencies
- Interval Scheduling: Various sorting criteria based on goal",
                triggerSignals: ToJson(new[] { "Maximize/minimize with constraints", "Interval problems", "Choose items with cost/value", "Local decision doesn't affect future options", "Sorting might help" }),
                commonMistakes: ToJson(new[] { "Applying greedy when DP is needed", "Wrong sorting criteria", "Not proving greedy works", "Missing edge cases", "Not considering ties properly" }),
                resources: ToJson(new object[] {
                    new { title = "Greedy Algorithms", url = "https://www.geeksforgeeks.org/greedy-algorithms/", type = "article" },
                    new { title = "Greedy - TakeUForward", url = "https://takeuforward.org/data-structure/greedy-algorithms/", type = "article" },
                    new { title = "Greedy - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/14", type = "course" },
                    new { title = "Greedy Problems - LeetCode", url = "https://leetcode.com/tag/greedy/", type = "practice" },
                    new { title = "Greedy - Abdul Bari", url = "https://www.youtube.com/watch?v=ARvQcqJ_-NY", type = "video" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Monotonic Stack Pattern
            Pattern.Create(
                name: "Monotonic Stack",
                description: "Maintain a stack with elements in sorted order to efficiently solve next greater/smaller element problems.",
                category: PatternCategory.MonotonicStack,
                whatItIs: @"A Monotonic Stack maintains elements in increasing or decreasing order. When a new element breaks the monotonic property, we pop elements and process them. This reveals relationships between elements (next greater, previous smaller, etc.).",
                whenToUse: @"Use Monotonic Stack when:
- Finding next/previous greater/smaller element
- Problems involving spans (stock span, largest rectangle)
- Tracking increasing/decreasing sequences
- Need O(1) amortized processing per element",
                whyItWorks: @"Monotonic Stack works because:
1. Each element is pushed and popped at most once: O(n) total
2. The monotonic property ensures when we pop, we've found the answer
3. Stack maintains a 'waiting list' of elements seeking their answer",
                commonUseCases: ToJson(new[] { "Next greater element", "Daily temperatures", "Largest rectangle in histogram", "Trapping rain water", "Stock span problem", "Sum of subarray minimums" }),
                timeComplexity: "O(n) - each element pushed/popped once",
                spaceComplexity: "O(n)",
                pseudoCode: @"MONOTONIC STACK ALGORITHM

PURPOSE: Efficiently find next/previous greater/smaller elements

MONOTONIC INCREASING STACK (for Next Greater Element):
1. Initialize empty stack and result array (fill with -1)
2. FOR each index i from 0 to n-1:
   a. WHILE stack not empty AND current > element at stack top:
      - Pop index from stack
      - The current element is the 'next greater' for popped index
      - Record in result array
   b. Push current index onto stack
3. Remaining indices in stack have no next greater element

MONOTONIC DECREASING STACK (for Next Smaller Element):
- Same logic but pop when current < stack top element

KEY VARIATIONS:
- Next Greater: Iterate left to right, pop when current > top
- Previous Greater: Iterate left to right, answer is current stack top
- Next Smaller: Iterate left to right, pop when current < top
- Previous Smaller: Iterate left to right, answer is current stack top

COMMON APPLICATIONS:
- Stock span problems
- Largest rectangle in histogram
- Trapping rain water
- Daily temperatures",
                triggerSignals: ToJson(new[] { "Next greater/smaller element", "Previous greater/smaller", "Span problems", "Histogram rectangle", "Temperature/stock problems", "Subarray min/max sums" }),
                commonMistakes: ToJson(new[] { "Wrong monotonic direction (increasing vs decreasing)", "Storing values instead of indices when indices needed", "Not handling remaining elements in stack", "Off-by-one errors" }),
                resources: ToJson(new object[] {
                    new { title = "Monotonic Stack - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/10", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Union-Find Pattern
            Pattern.Create(
                name: "Union-Find (Disjoint Set)",
                description: "Track connected components with near O(1) union and find operations using path compression and union by rank.",
                category: PatternCategory.UnionFind,
                whatItIs: @"Union-Find (Disjoint Set Union) maintains a collection of disjoint sets with operations:
- Find: Determine which set an element belongs to
- Union: Merge two sets

With path compression and union by rank, both operations are nearly O(1) (amortized).",
                whenToUse: @"Use Union-Find when:
- Tracking connected components dynamically
- Determining if elements are in same group
- Kruskal's MST algorithm
- Problems involving merging groups
- Cycle detection in undirected graphs",
                whyItWorks: @"Union-Find works because:
1. Path compression flattens the tree during find
2. Union by rank keeps trees balanced
3. Together they achieve O(α(n)) amortized time (inverse Ackermann, practically constant)",
                commonUseCases: ToJson(new[] { "Number of connected components", "Redundant connection", "Accounts merge", "Longest consecutive sequence", "Making a large island", "Satisfiability of equality equations" }),
                timeComplexity: "O(α(n)) ≈ O(1) per operation",
                spaceComplexity: "O(n)",
                pseudoCode: @"UNION-FIND (DISJOINT SET UNION) ALGORITHM

DATA STRUCTURE:
- parent[]: Each element points to its parent (or itself if root)
- rank[]: Height/size of each tree (for optimization)

FIND OPERATION (with path compression):
1. IF element is its own parent, it's the root - return it
2. OTHERWISE, recursively find root of parent
3. PATH COMPRESSION: Point element directly to root
4. Return the root

UNION OPERATION (with union by rank):
1. Find roots of both elements
2. IF same root, already connected - return false
3. ATTACH smaller tree under larger tree:
   - Compare ranks of both roots
   - Make higher-rank root the parent
   - If equal rank, pick one and increment its rank
4. Return true (union performed)

OPTIMIZATIONS:
- Path Compression: Flattens tree during find
- Union by Rank/Size: Keeps trees balanced
- Together: Nearly O(1) amortized per operation

APPLICATIONS:
- Connected components
- Cycle detection in undirected graphs
- Kruskal's MST algorithm
- Account merging / grouping problems",
                triggerSignals: ToJson(new[] { "Connected components", "Group membership", "Merge groups", "Dynamic connectivity", "Cycle detection undirected graph" }),
                commonMistakes: ToJson(new[] { "Forgetting path compression", "Not using union by rank", "Incorrect parent initialization", "Not handling self-loops" }),
                resources: ToJson(new object[] {
                    new { title = "Union Find - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/9", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Trie Pattern  
            Pattern.Create(
                name: "Trie (Prefix Tree)",
                description: "Tree structure for efficient storage and retrieval of strings, enabling prefix-based operations.",
                category: PatternCategory.Trie,
                whatItIs: @"A Trie is a tree where each node represents a character. Paths from root to nodes form prefixes of stored strings. Each node can have up to 26 children (for lowercase English) and a flag marking end of word.",
                whenToUse: @"Use Trie when:
- Need prefix-based search (autocomplete)
- Storing and searching a dictionary of words
- Finding words with common prefix
- Word search in a grid
- Implementing spell checker",
                whyItWorks: @"Trie works because:
1. Common prefixes are stored once, saving space
2. Search time is O(m) where m is word length, independent of dictionary size
3. All words with a prefix can be found by traversing subtree",
                commonUseCases: ToJson(new[] { "Implement Trie", "Word search II", "Search suggestions system", "Design add and search words", "Replace words", "Palindrome pairs" }),
                timeComplexity: "O(m) per operation where m is word length",
                spaceComplexity: "O(total characters in all words)",
                pseudoCode: @"TRIE (PREFIX TREE) ALGORITHM

DATA STRUCTURE:
- Each node contains:
  - children: Map/array of child nodes (one per character)
  - isEndOfWord: Boolean flag marking complete words

INSERT OPERATION:
1. Start at root node
2. FOR each character in word:
   a. IF child for character doesn't exist:
      - Create new node for character
   b. Move to child node
3. Mark final node as end of word

SEARCH OPERATION:
1. Start at root node
2. FOR each character in word:
   a. IF child for character doesn't exist:
      - Return false (word not found)
   b. Move to child node
3. Return true only if final node is marked as end of word

STARTS-WITH (PREFIX) OPERATION:
1. Same as search, but return true after traversing all chars
   (don't check isEndOfWord flag)

DELETE OPERATION:
1. Recursively traverse to end of word
2. Unmark end of word
3. Delete nodes that have no other children (bottom-up)",
                triggerSignals: ToJson(new[] { "Prefix search", "Autocomplete", "Word dictionary", "Multiple string search", "Common prefix operations" }),
                commonMistakes: ToJson(new[] { "Forgetting is_end flag", "Not handling empty strings", "Memory inefficiency with sparse tries", "Case sensitivity issues" }),
                resources: ToJson(new object[] {
                    new { title = "Trie - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/11", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Intervals Pattern
            Pattern.Create(
                name: "Intervals",
                description: "Techniques for handling overlapping intervals: merging, finding gaps, and scheduling.",
                category: PatternCategory.Intervals,
                whatItIs: @"Interval problems involve ranges [start, end]. Common operations:
- Merge overlapping intervals
- Find if intervals overlap
- Insert interval
- Find gaps between intervals

Key technique: Sort by start (or end) time, then process linearly.",
                whenToUse: @"Use Interval techniques when:
- Dealing with time ranges or segments
- Meeting room scheduling
- Merging or splitting ranges
- Finding overlaps or conflicts
- Calendar/booking problems",
                whyItWorks: @"Interval techniques work because:
1. Sorting allows linear processing
2. After sorting, overlaps can be detected by comparing adjacent intervals
3. Greedy choices (by end time) often yield optimal solutions",
                commonUseCases: ToJson(new[] { "Merge intervals", "Insert interval", "Meeting rooms", "Non-overlapping intervals", "Minimum number of arrows", "Employee free time" }),
                timeComplexity: "O(n log n) due to sorting",
                spaceComplexity: "O(n) for result, O(1) extra",
                pseudoCode: @"INTERVAL MERGING ALGORITHM

1. SORT intervals by start time (ascending)

2. INITIALIZE result with first interval

3. FOR each remaining interval [start, end]:
   a. Get last interval in result: [lastStart, lastEnd]
   b. IF start ≤ lastEnd (overlaps or touches):
      - MERGE: Update lastEnd = max(lastEnd, end)
   c. ELSE (gap exists):
      - ADD [start, end] as new interval to result

4. RETURN merged intervals

CHECKING OVERLAP:
Two intervals [a,b] and [c,d] overlap if: a ≤ d AND c ≤ b

COMMON VARIATIONS:
- Insert Interval: Binary search for position, then merge
- Meeting Rooms: Sort by start, check any overlap exists
- Meeting Rooms II: Use min-heap to track end times, or sweep line
- Non-overlapping: Greedy by end time, count removals needed",
                triggerSignals: ToJson(new[] { "Time ranges", "Overlapping intervals", "Merge ranges", "Meeting scheduling", "Booking conflicts", "Minimum operations on ranges" }),
                commonMistakes: ToJson(new[] { "Wrong sort key", "Not handling touching intervals correctly", "Off-by-one with inclusive/exclusive bounds", "Not merging all overlaps" }),
                resources: ToJson(new object[] {
                    new { title = "Intervals - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/12", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Divide and Conquer Pattern
            Pattern.Create(
                name: "Divide and Conquer",
                description: "Break problem into smaller subproblems, solve recursively, and combine results.",
                category: PatternCategory.DivideAndConquer,
                whatItIs: @"Divide and Conquer is a paradigm that breaks a problem into smaller independent subproblems, solves them recursively, and combines the results. Unlike DP, subproblems don't overlap.

Three steps:
1. Divide: Split into smaller subproblems
2. Conquer: Solve subproblems recursively
3. Combine: Merge subproblem solutions",
                whenToUse: @"Use Divide and Conquer when:
- Problem can be broken into independent subproblems
- Subproblems are similar to the original problem
- Solutions can be combined efficiently
- Classic examples: sorting, searching, matrix multiplication",
                whyItWorks: @"Divide and Conquer works because:
1. Smaller problems are easier to solve
2. Recursion naturally handles the division
3. Combining is often O(n) leading to O(n log n) total
4. Can be parallelized since subproblems are independent",
                commonUseCases: ToJson(new[] { "Merge Sort", "Quick Sort", "Binary Search", "Maximum subarray (Kadane)", "Closest pair of points", "Strassen's matrix multiplication", "Karatsuba multiplication" }),
                timeComplexity: "Varies: O(n log n) for sorting, O(log n) for search",
                spaceComplexity: "O(log n) to O(n) for recursion stack",
                pseudoCode: @"DIVIDE AND CONQUER ALGORITHM

GENERAL FRAMEWORK:
1. BASE CASE: If problem is small enough, solve directly

2. DIVIDE: Split problem into smaller subproblems
   - Typically divide in half (binary division)
   - Subproblems should be independent

3. CONQUER: Recursively solve each subproblem
   - Apply same algorithm to each smaller problem

4. COMBINE: Merge subproblem solutions into final answer
   - This step often determines overall complexity

MERGE SORT EXAMPLE:
1. Base: If array has ≤ 1 element, it's sorted
2. Divide: Split array into two halves at midpoint
3. Conquer: Recursively sort each half
4. Combine: Merge two sorted halves into one sorted array
   - Compare elements from both halves
   - Take smaller element each time
   - Results in O(n) merge, O(n log n) total

MASTER THEOREM: T(n) = aT(n/b) + O(n^d)
- a = number of subproblems
- b = factor by which problem size shrinks
- d = exponent in combine step",
                triggerSignals: ToJson(new[] { "Problem can be halved", "Subproblems are independent", "Sorting algorithms", "Matrix operations", "Geometric algorithms" }),
                commonMistakes: ToJson(new[] { "Wrong base case", "Inefficient combine step", "Not handling odd/even splits", "Stack overflow on deep recursion", "Using when DP is needed (overlapping subproblems)" }),
                resources: ToJson(new object[] {
                    new { title = "Divide and Conquer - Khan Academy", url = "https://www.khanacademy.org/computing/computer-science/algorithms/merge-sort/a/divide-and-conquer-algorithms", type = "article" },
                    new { title = "Divide and Conquer - GeeksforGeeks", url = "https://www.geeksforgeeks.org/divide-and-conquer/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Topological Sort Pattern
            Pattern.Create(
                name: "Topological Sort",
                description: "Linear ordering of vertices in a DAG where every directed edge u→v has u before v.",
                category: PatternCategory.Graph,
                whatItIs: @"Topological Sort produces a linear ordering of vertices in a Directed Acyclic Graph (DAG) such that for every edge (u, v), vertex u comes before v. Two approaches:
1. Kahn's Algorithm (BFS with indegree)
2. DFS with post-order reversal

Only possible for DAGs - existence of topological order proves no cycles.",
                whenToUse: @"Use Topological Sort when:
- Processing dependencies in order
- Build systems (compile order)
- Course prerequisites
- Task scheduling with dependencies
- Detecting cycles in directed graphs",
                whyItWorks: @"Topological Sort works because:
1. In a DAG, there's always a vertex with no incoming edges
2. Removing it and repeating gives valid ordering
3. DFS post-order naturally respects dependencies",
                commonUseCases: ToJson(new[] { "Course Schedule", "Alien Dictionary", "Build order", "Task scheduling", "Recipe steps", "Package dependencies" }),
                timeComplexity: "O(V + E)",
                spaceComplexity: "O(V) for result and auxiliary structures",
                pseudoCode: @"TOPOLOGICAL SORT ALGORITHMS

KAHN'S ALGORITHM (BFS-based):
1. CALCULATE in-degree for each node
   (count of edges pointing TO each node)

2. ADD all nodes with in-degree 0 to queue
   (these have no dependencies)

3. WHILE queue is not empty:
   a. Remove node from queue, add to result
   b. FOR each neighbor of this node:
      - Decrement neighbor's in-degree
      - IF neighbor's in-degree becomes 0:
        - Add neighbor to queue

4. IF result contains all nodes: return result
   ELSE: cycle exists (impossible to sort)

DFS-BASED APPROACH:
1. Do DFS, tracking visited and 'in current path'
2. Add node to result AFTER processing all neighbors
3. Reverse the result at the end
4. If 'in current path' node visited again: cycle detected

CYCLE DETECTION: If result length < number of nodes,
a cycle exists and topological sort is impossible.",
                triggerSignals: ToJson(new[] { "Dependencies/prerequisites", "Build order", "Course schedule", "DAG processing", "Cycle detection directed graph" }),
                commonMistakes: ToJson(new[] { "Forgetting to check for cycles", "Wrong indegree calculation", "Not handling disconnected components", "Confusing with DFS/BFS traversal" }),
                resources: ToJson(new object[] {
                    new { title = "Topological Sort - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/14", type = "course" },
                    new { title = "Kahn's Algorithm", url = "https://www.geeksforgeeks.org/topological-sorting-indegree-based-solution/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Bit Manipulation Pattern
            Pattern.Create(
                name: "Bit Manipulation",
                description: "Use bitwise operations to solve problems efficiently with constant space.",
                category: PatternCategory.BitManipulation,
                whatItIs: @"Bit manipulation uses bitwise operators (AND, OR, XOR, NOT, shifts) to solve problems at the bit level. Key operations:
- x & (x-1): Clear lowest set bit
- x & (-x): Get lowest set bit
- x ^ x = 0: XOR of same numbers is 0
- x ^ 0 = x: XOR with 0 preserves value

Enables O(1) space solutions for certain problems.",
                whenToUse: @"Use Bit Manipulation when:
- Finding single/unique numbers
- Power of 2 checks
- Counting bits
- Generating subsets
- Space optimization (bit vectors)",
                whyItWorks: @"Bit Manipulation works because:
1. XOR has properties: x^x=0, x^0=x, associative
2. Numbers can encode multiple boolean states
3. Bitwise operations are O(1) CPU operations
4. Bits can represent sets efficiently",
                commonUseCases: ToJson(new[] { "Single Number", "Number of 1 Bits", "Power of Two", "Counting Bits", "Subsets generation", "Missing Number", "Reverse Bits" }),
                timeComplexity: "O(1) to O(n) depending on problem",
                spaceComplexity: "Often O(1)",
                pseudoCode: @"BIT MANIPULATION ALGORITHMS

COMMON BIT OPERATIONS:
- n AND (n-1): Clear the lowest set bit
- n AND (-n): Isolate the lowest set bit
- n OR (1 << i): Set bit at position i
- n AND NOT(1 << i): Clear bit at position i
- (n >> i) AND 1: Check if bit at position i is set
- n XOR n = 0: XOR of same number is zero
- n XOR 0 = n: XOR with zero preserves value

SINGLE NUMBER (find element appearing once):
1. Initialize result = 0
2. FOR each number in array:
   result = result XOR number
3. Return result
(Pairs cancel out via XOR, unique remains)

COUNT SET BITS (Brian Kernighan's Algorithm):
1. Initialize count = 0
2. WHILE n > 0:
   n = n AND (n-1)  // Clears lowest set bit
   count = count + 1
3. Return count

GENERATE ALL SUBSETS using bits:
FOR mask from 0 to (2^n - 1):
  Current subset includes element i if bit i of mask is set",
                triggerSignals: ToJson(new[] { "Find unique/single number", "Power of 2", "Binary representation", "Subsets/combinations", "O(1) space requirement", "Toggle states" }),
                commonMistakes: ToJson(new[] { "Sign bit issues with negative numbers", "Wrong shift direction", "Overflow in bit shifts", "Not handling zero case" }),
                resources: ToJson(new object[] {
                    new { title = "Bit Manipulation - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/15", type = "course" },
                    new { title = "Bit Manipulation Tutorial", url = "https://leetcode.com/discuss/general-discussion/1073221/all-about-bitwise-operations-beginner-intermediate", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Prefix Sum Pattern
            Pattern.Create(
                name: "Prefix Sum",
                description: "Precompute cumulative sums to answer range sum queries in O(1).",
                category: PatternCategory.PrefixSum,
                whatItIs: @"Prefix Sum creates an array where each element is the sum of all previous elements plus itself. This allows O(1) range sum queries:
- prefix[j] - prefix[i-1] = sum(arr[i:j+1])

Can be extended to 2D (prefix sum matrix) and other operations (product, XOR).",
                whenToUse: @"Use Prefix Sum when:
- Multiple range sum queries
- Finding subarrays with target sum
- Cumulative statistics needed
- Range operations on arrays
- 2D matrix region queries",
                whyItWorks: @"Prefix Sum works because:
1. Precomputation amortizes over many queries
2. Range sum = difference of two prefix sums
3. Avoids O(n) computation per query",
                commonUseCases: ToJson(new[] { "Range Sum Query", "Subarray Sum Equals K", "Contiguous Array", "Product of Array Except Self", "2D Range Sum", "Running sum" }),
                timeComplexity: "O(n) precompute, O(1) query",
                spaceComplexity: "O(n) for prefix array",
                pseudoCode: @"PREFIX SUM ALGORITHM

BUILDING PREFIX SUM ARRAY:
1. Create array prefix of size n+1
2. Set prefix[0] = 0
3. FOR i from 0 to n-1:
   prefix[i+1] = prefix[i] + arr[i]

RANGE SUM QUERY [i, j]:
- Return prefix[j+1] - prefix[i]
- This gives sum of arr[i] through arr[j]

SUBARRAY SUM EQUALS K (using hash map):
1. Initialize count = 0, current_sum = 0
2. Create hash map seen = {0: 1}
3. FOR each number in array:
   a. Add number to current_sum
   b. IF (current_sum - k) exists in seen:
      - Add seen[current_sum - k] to count
   c. Add current_sum to seen (increment count)
4. Return count

KEY INSIGHT: If prefix[j] - prefix[i] = k, then
subarray from i to j-1 sums to k.

2D PREFIX SUM:
prefix[i][j] = sum of rectangle from (0,0) to (i-1,j-1)
Region sum uses inclusion-exclusion principle",
                triggerSignals: ToJson(new[] { "Range sum queries", "Subarray with target sum", "Cumulative operations", "Multiple queries on same array", "2D matrix region sums" }),
                commonMistakes: ToJson(new[] { "Off-by-one in prefix indexing", "Not handling empty ranges", "Integer overflow with large sums", "Forgetting prefix[0] = 0" }),
                resources: ToJson(new object[] {
                    new { title = "Prefix Sum - LeetCode", url = "https://leetcode.com/problems/range-sum-query-immutable/", type = "problem" },
                    new { title = "Prefix Sum Tutorial", url = "https://www.geeksforgeeks.org/prefix-sum-array-implementation-applications-competitive-programming/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Fast and Slow Pointers Pattern
            Pattern.Create(
                name: "Fast and Slow Pointers (Floyd's)",
                description: "Use two pointers moving at different speeds to detect cycles and find middle elements.",
                category: PatternCategory.TwoPointers,
                whatItIs: @"Fast and Slow Pointers (Floyd's Tortoise and Hare) uses two pointers moving at different speeds:
- Slow pointer moves 1 step
- Fast pointer moves 2 steps

If there's a cycle, they'll meet. Also useful for finding middle of linked list in one pass.",
                whenToUse: @"Use Fast/Slow Pointers when:
- Detecting cycles in linked lists
- Finding middle of linked list
- Finding start of cycle
- Happy number problem
- Detecting loops in sequences",
                whyItWorks: @"Fast/Slow Pointers works because:
1. In a cycle, fast catches up to slow by 1 step each iteration
2. They meet within the cycle's length iterations
3. Mathematical property: meeting point relates to cycle start",
                commonUseCases: ToJson(new[] { "Linked List Cycle", "Linked List Cycle II", "Find Middle of List", "Happy Number", "Find Duplicate Number", "Palindrome Linked List" }),
                timeComplexity: "O(n)",
                spaceComplexity: "O(1)",
                pseudoCode: @"FAST AND SLOW POINTERS (FLOYD'S ALGORITHM)

CYCLE DETECTION:
1. Initialize slow and fast pointers at head
2. WHILE fast is not null AND fast.next is not null:
   a. Move slow one step forward
   b. Move fast two steps forward
   c. IF slow == fast: cycle detected, return true
3. If loop ends, no cycle - return false

FIND CYCLE START (after detecting cycle):
1. After slow and fast meet, reset slow to head
2. Move both pointers one step at a time
3. They will meet at the cycle start

WHY THIS WORKS: When fast catches slow inside cycle,
the distance from head to cycle start equals distance
from meeting point to cycle start (going around cycle).

FIND MIDDLE OF LIST:
1. Initialize slow and fast at head
2. WHILE fast is not null AND fast.next is not null:
   - Move slow one step
   - Move fast two steps
3. When fast reaches end, slow is at middle

For even-length lists, this gives the second middle node.",
                triggerSignals: ToJson(new[] { "Cycle detection", "Find middle element", "Linked list loop", "Sequence repeats", "O(1) space with linked list" }),
                commonMistakes: ToJson(new[] { "Not checking fast.next before fast.next.next", "Wrong initialization", "Not handling empty list", "Confusing cycle detection with finding cycle start" }),
                resources: ToJson(new object[] {
                    new { title = "Floyd's Algorithm", url = "https://www.geeksforgeeks.org/floyds-cycle-finding-algorithm/", type = "article" },
                    new { title = "Linked List Cycle - LeetCode", url = "https://leetcode.com/problems/linked-list-cycle/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Matrix Traversal Pattern
            Pattern.Create(
                name: "Matrix Traversal",
                description: "Techniques for traversing 2D grids: DFS, BFS, spiral, diagonal patterns.",
                category: PatternCategory.Matrix,
                whatItIs: @"Matrix Traversal covers techniques for navigating 2D arrays:
- 4-directional: up, down, left, right
- 8-directional: including diagonals
- Spiral order: outer to inner
- Diagonal traversal

Key: direction arrays simplify code.",
                whenToUse: @"Use Matrix Traversal when:
- Grid-based pathfinding
- Image processing (flood fill)
- Game boards
- Finding connected regions
- Spiral/diagonal output",
                whyItWorks: @"Matrix Traversal works because:
1. Direction arrays standardize movement
2. BFS finds shortest path in unweighted grids
3. DFS explores all connected cells
4. Boundary checks prevent index errors",
                commonUseCases: ToJson(new[] { "Number of Islands", "Flood Fill", "Rotting Oranges", "Word Search", "Spiral Matrix", "Diagonal Traverse", "Shortest Path in Grid" }),
                timeComplexity: "O(m × n) to visit all cells",
                spaceComplexity: "O(m × n) for visited array or O(min(m,n)) for BFS queue",
                pseudoCode: @"MATRIX TRAVERSAL ALGORITHMS

DIRECTION ARRAYS:
- 4 directions: (0,1), (0,-1), (1,0), (-1,0) [right, left, down, up]
- 8 directions: Add diagonals (-1,-1), (-1,1), (1,-1), (1,1)

BFS ON GRID (for shortest path):
1. Initialize queue with starting position
2. Mark start as visited
3. WHILE queue not empty:
   a. Remove position (row, col) from queue
   b. FOR each direction (dr, dc):
      - Calculate new position (nr, nc) = (row+dr, col+dc)
      - IF in bounds AND not visited AND valid cell:
        - Mark as visited
        - Add to queue

DFS ON GRID (for exploring regions):
1. Mark current cell as visited
2. FOR each valid neighbor:
   - Recursively call DFS

SPIRAL TRAVERSAL:
1. Track four boundaries: top, bottom, left, right
2. WHILE boundaries don't cross:
   - Traverse right along top row, then top++
   - Traverse down along right column, then right--
   - Traverse left along bottom row, then bottom--
   - Traverse up along left column, then left++

KEY: Always check bounds before accessing grid[r][c]",
                triggerSignals: ToJson(new[] { "2D grid/matrix", "Find connected regions", "Shortest path in grid", "Flood fill", "Spiral/diagonal pattern", "Island problems" }),
                commonMistakes: ToJson(new[] { "Index out of bounds", "Not marking visited before enqueueing", "Wrong boundary conditions", "Modifying grid while iterating" }),
                resources: ToJson(new object[] {
                    new { title = "Matrix Traversal Patterns", url = "https://leetcode.com/discuss/general-discussion/657507/", type = "article" },
                    new { title = "Number of Islands", url = "https://leetcode.com/problems/number-of-islands/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Top K Elements Pattern
            Pattern.Create(
                name: "Top K Elements",
                description: "Find k largest, smallest, or most frequent elements using heaps or quickselect.",
                category: PatternCategory.Heap,
                whatItIs: @"Top K Elements finds the k largest, smallest, or most frequent elements. Main approaches:
1. Heap of size k: O(n log k)
2. QuickSelect: O(n) average
3. Bucket sort for frequency: O(n)

Use min-heap for k largest, max-heap for k smallest.",
                whenToUse: @"Use Top K when:
- Finding k largest/smallest elements
- Finding k most/least frequent
- K closest points
- Need partial sorting only",
                whyItWorks: @"Top K works because:
1. Heap maintains only k elements, reducing log factor
2. QuickSelect partitions around kth element
3. Don't need full sort - only position relative to k matters",
                commonUseCases: ToJson(new[] { "Kth Largest Element", "Top K Frequent Elements", "K Closest Points", "Find K Pairs with Smallest Sums", "Kth Largest in Stream" }),
                timeComplexity: "O(n log k) with heap, O(n) with quickselect",
                spaceComplexity: "O(k)",
                pseudoCode: @"TOP K ELEMENTS ALGORITHMS

APPROACH 1: MIN-HEAP OF SIZE K (for K largest)
1. Initialize empty min-heap
2. FOR each element in array:
   a. Add element to heap
   b. IF heap size > k:
      - Remove minimum (root) from heap
3. Heap now contains k largest elements
   (Smallest of the k largest is at root)

WHY MIN-HEAP for LARGEST? The min-heap naturally evicts
the smallest element, keeping only the k largest.

APPROACH 2: QUICKSELECT (for Kth element)
1. Choose a pivot element
2. Partition: elements < pivot go left, > pivot go right
3. IF pivot is at position k: return it
   ELIF k is in left partition: recurse left
   ELSE: recurse right
4. Average O(n), worst O(n²)

TOP K FREQUENT:
1. Count frequencies using hash map
2. Use heap of size k on (frequency, element) pairs
   OR use bucket sort: bucket[freq] = [elements with that freq]

K SMALLEST: Use max-heap of size k instead
K CLOSEST POINTS: Use max-heap with distances",
                triggerSignals: ToJson(new[] { "K largest/smallest", "K most frequent", "K closest", "Partial sorting", "Stream of elements" }),
                commonMistakes: ToJson(new[] { "Using wrong heap type (min vs max)", "Sorting when k << n", "Not handling k > n", "QuickSelect worst case O(n²)" }),
                resources: ToJson(new object[] {
                    new { title = "Top K Elements - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/16", type = "course" },
                    new { title = "Kth Largest Element", url = "https://leetcode.com/problems/kth-largest-element-in-an-array/", type = "problem" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // K-way Merge Pattern
            Pattern.Create(
                name: "K-way Merge",
                description: "Merge K sorted arrays or lists efficiently using a min-heap.",
                category: PatternCategory.Heap,
                whatItIs: @"K-way Merge efficiently combines K sorted sequences into one sorted output. Uses a min-heap to always get the smallest current element across all lists. Each list contributes one element to the heap at a time.",
                whenToUse: @"Use K-way Merge when:
- Merging k sorted arrays/lists
- External sorting (merge sorted chunks)
- Finding smallest range covering elements from k lists
- K sorted streams",
                whyItWorks: @"K-way Merge works because:
1. Heap maintains k elements (one from each list)
2. Pop gives global minimum
3. Push replacement maintains invariant
4. Total n elements × log k heap operations",
                commonUseCases: ToJson(new[] { "Merge K Sorted Lists", "Smallest Range Covering Elements", "Kth Smallest in Sorted Matrix", "Find K Pairs with Smallest Sums", "External sort merge phase" }),
                timeComplexity: "O(n log k) where n = total elements, k = number of lists",
                spaceComplexity: "O(k) for heap",
                pseudoCode: @"K-WAY MERGE ALGORITHM

1. INITIALIZE min-heap with first element from each list
   - Store tuple: (value, list_index, element_index)
   - This identifies where each element came from

2. WHILE heap is not empty:
   a. EXTRACT minimum element from heap
   b. ADD this element to result
   c. IF the list this element came from has more elements:
      - PUSH next element from that list into heap

3. RETURN merged result

KEY INSIGHT: Heap always contains at most k elements (one from
each list), so heap operations are O(log k). Total: O(n log k).

FOR LINKED LISTS:
- Store (node.val, list_index, node) in heap
- After extracting, push node.next if it exists

FOR ARRAYS:
- Store (value, array_index, element_index)
- After extracting, push next element if within bounds

SMALLEST RANGE COVERING K LISTS:
- Track current max while heap tracks min
- Range = [heap_min, current_max]
- Shrink by advancing the list that contributed min",
                triggerSignals: ToJson(new[] { "Merge k sorted", "Multiple sorted streams", "External sorting", "Smallest from k lists", "Global order from local orders" }),
                commonMistakes: ToJson(new[] { "Not tracking list index in heap", "Comparing nodes directly (need val)", "Not handling empty lists", "Forgetting to check for next element" }),
                resources: ToJson(new object[] {
                    new { title = "Merge K Sorted Lists", url = "https://leetcode.com/problems/merge-k-sorted-lists/", type = "problem" },
                    new { title = "K-way Merge Pattern", url = "https://www.educative.io/courses/grokking-the-coding-interview/Y5n0n3vAgYK", type = "course" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Shortest Path Pattern
            Pattern.Create(
                name: "Shortest Path Algorithms",
                description: "Find shortest paths in weighted graphs: Dijkstra, Bellman-Ford, Floyd-Warshall.",
                category: PatternCategory.Graph,
                whatItIs: @"Shortest Path algorithms find minimum-cost paths in weighted graphs:
- Dijkstra: Single source, non-negative weights, O((V+E)log V)
- Bellman-Ford: Single source, handles negative weights, O(VE)
- Floyd-Warshall: All pairs, O(V³)

Choice depends on graph properties and query needs.",
                whenToUse: @"Use Dijkstra when:
- Non-negative edge weights
- Single source shortest paths
- Need efficient O((V+E)log V)

Use Bellman-Ford when:
- Negative edge weights possible
- Need to detect negative cycles

Use Floyd-Warshall when:
- Need all pairs shortest paths
- Dense graph (E ≈ V²)",
                whyItWorks: @"These algorithms work because:
1. Dijkstra: Greedy selection of minimum distance vertex is safe with non-negative weights
2. Bellman-Ford: V-1 iterations relax all edges, enough for any shortest path
3. Floyd-Warshall: Dynamic programming on intermediate vertices",
                commonUseCases: ToJson(new[] { "Network Delay Time", "Cheapest Flights Within K Stops", "Path with Minimum Effort", "Swim in Rising Water", "All pairs shortest paths" }),
                timeComplexity: "Dijkstra: O((V+E)log V), Bellman-Ford: O(VE), Floyd-Warshall: O(V³)",
                spaceComplexity: "O(V) for Dijkstra/Bellman-Ford, O(V²) for Floyd-Warshall",
                pseudoCode: @"SHORTEST PATH ALGORITHMS

DIJKSTRA'S ALGORITHM (non-negative weights):
1. Initialize distance to all nodes as infinity, dist[start] = 0
2. Add (0, start) to min-heap
3. WHILE heap not empty:
   a. Extract node u with minimum distance d
   b. IF d > dist[u]: skip (outdated entry)
   c. FOR each neighbor v with edge weight w:
      - IF dist[u] + w < dist[v]:
        - Update dist[v] = dist[u] + w
        - Add (dist[v], v) to heap
4. Return distances

BELLMAN-FORD (handles negative weights):
1. Initialize dist[start] = 0, others = infinity
2. REPEAT (V-1) times:
   FOR each edge (u, v, w):
     IF dist[u] + w < dist[v]:
       dist[v] = dist[u] + w
3. DETECT negative cycle: Run one more iteration
   If any distance improves, negative cycle exists

FLOYD-WARSHALL (all pairs):
1. Initialize dist[i][j] = edge weight or infinity
2. FOR each intermediate vertex k:
   FOR each pair (i, j):
     dist[i][j] = min(dist[i][j], dist[i][k] + dist[k][j])",
                triggerSignals: ToJson(new[] { "Weighted graph", "Minimum cost path", "Network delay/latency", "Travel cost optimization", "Negative weights possible" }),
                commonMistakes: ToJson(new[] { "Using Dijkstra with negative weights", "Not detecting negative cycles", "Wrong priority queue usage", "Not handling disconnected nodes" }),
                resources: ToJson(new object[] {
                    new { title = "Dijkstra's Algorithm - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/17", type = "course" },
                    new { title = "Bellman-Ford Algorithm", url = "https://www.geeksforgeeks.org/bellman-ford-algorithm-dp-23/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // Minimum Spanning Tree Pattern
            Pattern.Create(
                name: "Minimum Spanning Tree",
                description: "Find tree connecting all vertices with minimum total edge weight: Kruskal's and Prim's algorithms.",
                category: PatternCategory.Graph,
                whatItIs: @"A Minimum Spanning Tree (MST) connects all vertices in an undirected weighted graph with minimum total edge weight. Two classic algorithms:
- Kruskal's: Sort edges, add if no cycle (uses Union-Find)
- Prim's: Grow tree from start vertex (uses heap)

Both produce optimal MST with V-1 edges.",
                whenToUse: @"Use MST when:
- Connecting all points with minimum cost
- Network design (cables, roads)
- Clustering (remove expensive edges)
- Approximating traveling salesman",
                whyItWorks: @"MST algorithms work because:
1. Cut property: lightest edge crossing any cut is in MST
2. Kruskal's greedily picks smallest safe edge
3. Prim's greedily expands tree with cheapest edge",
                commonUseCases: ToJson(new[] { "Min Cost to Connect All Points", "Network cable/road design", "Connecting cities", "Image segmentation", "Cluster analysis" }),
                timeComplexity: "Kruskal: O(E log E), Prim: O((V+E) log V)",
                spaceComplexity: "O(V) for Union-Find or heap",
                pseudoCode: @"MINIMUM SPANNING TREE ALGORITHMS

KRUSKAL'S ALGORITHM (edge-based):
1. SORT all edges by weight (ascending)
2. Initialize Union-Find with n nodes
3. Initialize MST weight = 0, edge count = 0
4. FOR each edge (u, v, w) in sorted order:
   a. IF u and v are in different components (Find):
      - Union u and v
      - Add w to MST weight
      - Increment edge count
   b. IF edge count == n-1: MST complete, stop
5. Return MST weight

PRIM'S ALGORITHM (vertex-based):
1. Initialize visited array (all false)
2. Add (0, start_node) to min-heap
3. Initialize MST weight = 0
4. WHILE heap not empty AND not all visited:
   a. Extract (weight, node) with minimum weight
   b. IF node already visited: skip
   c. Mark node visited, add weight to MST
   d. FOR each unvisited neighbor:
      - Add (edge_weight, neighbor) to heap
5. Return MST weight

CHOOSE KRUSKAL when: Sparse graph, edges given as list
CHOOSE PRIM when: Dense graph, adjacency list given",
                triggerSignals: ToJson(new[] { "Connect all points minimum cost", "Network design", "Spanning tree", "Minimum total weight to connect" }),
                commonMistakes: ToJson(new[] { "Using on directed graphs", "Not checking if all vertices connected", "Wrong Union-Find usage", "Duplicate edges in heap" }),
                resources: ToJson(new object[] {
                    new { title = "MST - Kruskal's", url = "https://www.geeksforgeeks.org/kruskals-minimum-spanning-tree-algorithm-greedy-algo-2/", type = "article" },
                    new { title = "MST - Prim's", url = "https://www.geeksforgeeks.org/prims-minimum-spanning-tree-mst-greedy-algo-5/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            ),

            // String Matching Pattern
            Pattern.Create(
                name: "String Matching Algorithms",
                description: "Efficient algorithms for finding patterns in text: KMP, Rabin-Karp, Z-algorithm.",
                category: PatternCategory.String,
                whatItIs: @"String matching algorithms find occurrences of a pattern in text:
- KMP (Knuth-Morris-Pratt): Uses failure function to avoid re-comparing, O(n+m)
- Rabin-Karp: Rolling hash for average O(n+m), good for multiple patterns
- Z-algorithm: Computes Z-array for pattern matching, O(n+m)

All achieve linear time vs naive O(nm).",
                whenToUse: @"Use KMP when:
- Single pattern, need guaranteed O(n+m)
- Pattern has repeating prefixes

Use Rabin-Karp when:
- Multiple patterns to search
- Substring matching problems

Use Z-algorithm when:
- Pattern matching or period finding",
                whyItWorks: @"These algorithms work because:
1. KMP: Failure function tells how far to shift without missing matches
2. Rabin-Karp: Rolling hash updates in O(1), only verify hash matches
3. Z-array: Precomputed prefix lengths enable O(1) matching decisions",
                commonUseCases: ToJson(new[] { "Find substring", "Pattern matching", "Repeated substrings", "String period", "Anagram search" }),
                timeComplexity: "O(n + m) where n = text length, m = pattern length",
                spaceComplexity: "O(m) for failure/Z array",
                pseudoCode: @"STRING MATCHING ALGORITHMS

KMP (KNUTH-MORRIS-PRATT) ALGORITHM:

Step 1: Build LPS (Longest Proper Prefix Suffix) array
- lps[i] = length of longest proper prefix of pattern[0..i]
          that is also a suffix
- Example: 'ABAB' -> lps = [0, 0, 1, 2]

Building LPS:
1. lps[0] = 0, length = 0, i = 1
2. WHILE i < pattern length:
   - IF pattern[i] == pattern[length]:
     length++, lps[i] = length, i++
   - ELIF length > 0:
     length = lps[length-1] (use previous LPS)
   - ELSE: lps[i] = 0, i++

Step 2: Search using LPS
1. i = 0 (text index), j = 0 (pattern index)
2. WHILE i < text length:
   - IF text[i] == pattern[j]: i++, j++
     - IF j == pattern length: Found match at i-j
   - ELIF j > 0: j = lps[j-1] (skip matched prefix)
   - ELSE: i++

KEY INSIGHT: When mismatch occurs, LPS tells us how many
characters we can skip (already matched as prefix).",
                triggerSignals: ToJson(new[] { "Find pattern in text", "Multiple pattern search", "String period/repetition", "Efficient substring search" }),
                commonMistakes: ToJson(new[] { "Wrong LPS/failure function", "Hash collision handling in Rabin-Karp", "Off-by-one in indices", "Not handling empty pattern" }),
                resources: ToJson(new object[] {
                    new { title = "KMP Algorithm", url = "https://www.geeksforgeeks.org/kmp-algorithm-for-pattern-searching/", type = "article" },
                    new { title = "Rabin-Karp", url = "https://www.geeksforgeeks.org/rabin-karp-algorithm-for-pattern-searching/", type = "article" }
                }),
                relatedPatternIds: ToJson(Array.Empty<Guid>())
            )
        ];
    }

    private static List<DataStructure> CreateDataStructures()
    {
        return
        [
            // Array
            DataStructure.Create(
                name: "Array",
                description: "Contiguous block of memory storing elements of the same type, accessed by index.",
                category: DataStructureCategory.Linear,
                whatItIs: @"An array is a fundamental data structure that stores elements in contiguous memory locations. Each element is accessed by its index (position) in constant time. Arrays have a fixed size in many languages, though dynamic arrays (like Python lists) can grow.",
                operations: ToJson(new object[] {
                    new { name = "Access by index", timeComplexity = "O(1)", description = "Get element at position i" },
                    new { name = "Update by index", timeComplexity = "O(1)", description = "Set element at position i" },
                    new { name = "Insert at end", timeComplexity = "O(1)*", description = "Amortized O(1) for dynamic arrays" },
                    new { name = "Insert at position", timeComplexity = "O(n)", description = "Shift elements right" },
                    new { name = "Delete at position", timeComplexity = "O(n)", description = "Shift elements left" },
                    new { name = "Search (unsorted)", timeComplexity = "O(n)", description = "Linear scan" },
                    new { name = "Search (sorted)", timeComplexity = "O(log n)", description = "Binary search" }
                }),
                whenToUse: @"Use arrays when:
- Need fast random access by index
- Data size is known or grows mainly at the end
- Memory locality matters for performance
- Implementing other data structures",
                tradeoffs: @"Pros:
- O(1) random access
- Memory efficient (no pointers)
- Cache-friendly (contiguous memory)

Cons:
- Fixed size (static arrays)
- Expensive insertion/deletion in middle
- May waste space if not full",
                commonUseCases: ToJson(new[] { "Storing collections", "Implementing stacks/queues", "Matrix operations", "Buffers", "Hash table backing" }),
                implementation: @"ARRAY OPERATIONS

ACCESS BY INDEX
  Input: array, index i
  Output: element at position i
  Time: O(1) - direct memory address calculation

UPDATE BY INDEX
  Input: array, index i, new value
  Action: Replace element at position i with new value
  Time: O(1) - direct memory access

INSERT AT END (Dynamic Array)
  1. IF array is full, allocate new array with 2x capacity
  2. Copy existing elements (only when resizing)
  3. Place new element at position [size]
  4. Increment size
  Time: O(1) amortized

INSERT AT POSITION i
  1. Shift all elements from position i to end one position right
  2. Place new element at position i
  3. Increment size
  Time: O(n) - must shift n-i elements

DELETE AT POSITION i
  1. Shift all elements from position i+1 to end one position left
  2. Decrement size
  Time: O(n) - must shift n-i-1 elements",
                resources: ToJson(new object[] {
                    new { title = "Arrays 101 - LeetCode", url = "https://leetcode.com/explore/learn/card/fun-with-arrays/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Linked List
            DataStructure.Create(
                name: "Linked List",
                description: "Linear collection where each element (node) contains data and reference to the next node.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A linked list is a linear data structure where elements (nodes) are not stored in contiguous memory. Each node contains data and a pointer to the next node. Variants include doubly linked lists (pointers to both next and previous) and circular linked lists.",
                operations: ToJson(new object[] {
                    new { name = "Access by index", timeComplexity = "O(n)", description = "Must traverse from head" },
                    new { name = "Insert at head", timeComplexity = "O(1)", description = "Update head pointer" },
                    new { name = "Insert at tail", timeComplexity = "O(1)*", description = "O(1) with tail pointer, O(n) without" },
                    new { name = "Insert after node", timeComplexity = "O(1)", description = "If node reference given" },
                    new { name = "Delete node", timeComplexity = "O(1)", description = "If node reference given" },
                    new { name = "Search", timeComplexity = "O(n)", description = "Linear traversal" }
                }),
                whenToUse: @"Use linked lists when:
- Frequent insertions/deletions at any position
- Don't need random access
- Don't know size in advance
- Implementing stacks, queues, or LRU cache",
                tradeoffs: @"Pros:
- O(1) insertion/deletion (with reference)
- Dynamic size
- No wasted space

Cons:
- O(n) random access
- Extra memory for pointers
- Poor cache locality",
                commonUseCases: ToJson(new[] { "LRU Cache", "Undo functionality", "Music playlist", "Browser history", "Polynomial representation" }),
                implementation: @"LINKED LIST OPERATIONS

NODE STRUCTURE
  Each node contains:
  - value: the data stored
  - next: pointer/reference to next node (null for last node)

INSERT AT HEAD
  1. Create new node with given value
  2. Set new node's next pointer to current head
  3. Update head to point to new node
  Time: O(1)

INSERT AT TAIL (with tail pointer)
  1. Create new node with given value
  2. Set current tail's next to new node
  3. Update tail to point to new node
  Time: O(1)

INSERT AFTER NODE
  1. Create new node with given value
  2. Set new node's next to given node's next
  3. Set given node's next to new node
  Time: O(1)

DELETE NODE (given previous node)
  1. Set previous node's next to node.next.next
  2. (Optional) Free deleted node's memory
  Time: O(1)

TRAVERSE/SEARCH
  1. Start at head
  2. While current node is not null:
     a. Process/check current node
     b. Move to current.next
  Time: O(n)",
                resources: ToJson(new object[] {
                    new { title = "Linked List - LeetCode", url = "https://leetcode.com/explore/learn/card/linked-list/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Stack
            DataStructure.Create(
                name: "Stack",
                description: "LIFO (Last-In-First-Out) collection supporting push and pop operations.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A stack is a linear data structure following Last-In-First-Out (LIFO) principle. The last element added is the first one removed. Think of a stack of plates - you add and remove from the top only.",
                operations: ToJson(new object[] {
                    new { name = "Push", timeComplexity = "O(1)", description = "Add element to top" },
                    new { name = "Pop", timeComplexity = "O(1)", description = "Remove and return top element" },
                    new { name = "Peek/Top", timeComplexity = "O(1)", description = "View top element without removing" },
                    new { name = "isEmpty", timeComplexity = "O(1)", description = "Check if stack is empty" },
                    new { name = "Size", timeComplexity = "O(1)", description = "Number of elements" }
                }),
                whenToUse: @"Use stacks when:
- Need LIFO ordering
- Tracking nested structures (parentheses, HTML tags)
- Undo operations
- DFS traversal
- Expression evaluation",
                tradeoffs: @"Pros:
- Simple interface
- All operations O(1)
- Perfect for LIFO patterns

Cons:
- No random access
- Can only access top element
- Fixed ordering",
                commonUseCases: ToJson(new[] { "Valid parentheses", "Reverse Polish notation", "Function call stack", "Browser back button", "Undo/Redo" }),
                implementation: @"STACK OPERATIONS (LIFO)

PUSH (add to top)
  1. IF using array: place element at position [top + 1], increment top
  2. IF using linked list: create new node, link to current head, update head
  Time: O(1)

POP (remove from top)
  1. IF stack is empty, return error/null
  2. Store top element for return
  3. IF using array: decrement top index
  4. IF using linked list: update head to head.next
  5. Return stored element
  Time: O(1)

PEEK (view top without removing)
  1. IF stack is empty, return error/null
  2. Return element at top position
  Time: O(1)

IS_EMPTY
  1. Return true if top index is -1 (array) or head is null (linked list)
  Time: O(1)",
                resources: ToJson(new object[] {
                    new { title = "Stack Problems", url = "https://leetcode.com/tag/stack/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Queue
            DataStructure.Create(
                name: "Queue",
                description: "FIFO (First-In-First-Out) collection supporting enqueue and dequeue operations.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A queue is a linear data structure following First-In-First-Out (FIFO) principle. The first element added is the first one removed. Think of a line at a store - first person in line is served first.",
                operations: ToJson(new object[] {
                    new { name = "Enqueue", timeComplexity = "O(1)", description = "Add element to back" },
                    new { name = "Dequeue", timeComplexity = "O(1)", description = "Remove and return front element" },
                    new { name = "Front/Peek", timeComplexity = "O(1)", description = "View front element" },
                    new { name = "isEmpty", timeComplexity = "O(1)", description = "Check if queue is empty" },
                    new { name = "Size", timeComplexity = "O(1)", description = "Number of elements" }
                }),
                whenToUse: @"Use queues when:
- Need FIFO ordering
- BFS traversal
- Task scheduling
- Buffering data streams
- Producer-consumer problems",
                tradeoffs: @"Pros:
- Simple interface
- All operations O(1)
- Natural for FIFO patterns

Cons:
- No random access
- Can only access front element",
                commonUseCases: ToJson(new[] { "BFS traversal", "Level-order tree traversal", "Task scheduling", "Print queue", "Message queues" }),
                implementation: @"QUEUE OPERATIONS (FIFO)

ENQUEUE (add to back)
  1. IF using circular array: place at position [(rear + 1) mod capacity], update rear
  2. IF using linked list: create new node, link current tail to new node, update tail
  Time: O(1)

DEQUEUE (remove from front)
  1. IF queue is empty, return error/null
  2. Store front element for return
  3. IF using circular array: update front = (front + 1) mod capacity
  4. IF using linked list: update head to head.next
  5. Return stored element
  Time: O(1)

PEEK (view front without removing)
  1. IF queue is empty, return error/null
  2. Return element at front position
  Time: O(1)

IS_EMPTY
  1. Return true if front equals rear (circular array) or head is null
  Time: O(1)",
                resources: ToJson(new object[] {
                    new { title = "Queue Problems", url = "https://leetcode.com/tag/queue/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Hash Table
            DataStructure.Create(
                name: "Hash Table (Dictionary)",
                description: "Key-value store with average O(1) lookup using hash function.",
                category: DataStructureCategory.HashBased,
                whatItIs: @"A hash table (dictionary/map) stores key-value pairs using a hash function to compute an index. Collisions are handled via chaining (linked lists) or open addressing. Provides near-constant time operations on average.",
                operations: ToJson(new object[] {
                    new { name = "Insert", timeComplexity = "O(1)*", description = "Average case, O(n) worst with collisions" },
                    new { name = "Delete", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Lookup", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Contains Key", timeComplexity = "O(1)*", description = "Average case" },
                    new { name = "Iterate", timeComplexity = "O(n)", description = "Visit all key-value pairs" }
                }),
                whenToUse: @"Use hash tables when:
- Need fast key-based lookup
- Counting frequencies
- Caching/memoization
- Detecting duplicates
- Grouping by key",
                tradeoffs: @"Pros:
- O(1) average operations
- Flexible keys
- Natural for counting/grouping

Cons:
- No ordering
- Worst case O(n) with collisions
- Memory overhead for load factor",
                commonUseCases: ToJson(new[] { "Two Sum", "Frequency counting", "Caching", "Anagram grouping", "LRU Cache", "Symbol tables" }),
                implementation: @"HASH TABLE OPERATIONS

HASH FUNCTION
  Input: key
  Output: index in range [0, capacity-1]
  Common: hash(key) mod capacity

INSERT (key, value)
  1. Compute index = hash(key) mod capacity
  2. IF collision (slot occupied by different key):
     - Chaining: add to linked list at index
     - Open addressing: probe for next empty slot
  3. Store key-value pair at index
  4. IF load factor > threshold, resize and rehash all entries
  Time: O(1) average, O(n) worst case

LOOKUP (key)
  1. Compute index = hash(key) mod capacity
  2. IF chaining: search linked list at index
  3. IF open addressing: probe until key found or empty slot
  4. Return value if found, else null/error
  Time: O(1) average

DELETE (key)
  1. Compute index = hash(key) mod capacity
  2. Find entry with matching key
  3. Remove entry (mark as deleted for open addressing)
  Time: O(1) average",
                resources: ToJson(new object[] {
                    new { title = "Hash Table - LeetCode", url = "https://leetcode.com/explore/learn/card/hash-table/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Hash Set
            DataStructure.Create(
                name: "Hash Set",
                description: "Collection of unique elements with O(1) average membership testing.",
                category: DataStructureCategory.HashBased,
                whatItIs: @"A hash set is a collection that stores unique elements with no duplicates. Uses hashing for fast membership testing. Elements must be hashable (immutable in Python).",
                operations: ToJson(new object[] {
                    new { name = "Add", timeComplexity = "O(1)*", description = "Add element (no-op if exists)" },
                    new { name = "Remove", timeComplexity = "O(1)*", description = "Remove element" },
                    new { name = "Contains", timeComplexity = "O(1)*", description = "Check membership" },
                    new { name = "Union", timeComplexity = "O(n+m)", description = "Combine two sets" },
                    new { name = "Intersection", timeComplexity = "O(min(n,m))", description = "Common elements" }
                }),
                whenToUse: @"Use hash sets when:
- Need unique elements only
- Fast membership testing
- Detecting duplicates
- Set operations (union, intersection)
- Tracking visited states",
                tradeoffs: @"Pros:
- O(1) membership test
- Automatic duplicate handling
- Set operations

Cons:
- No ordering
- Elements must be hashable
- Memory overhead",
                commonUseCases: ToJson(new[] { "Contains Duplicate", "Longest consecutive sequence", "Visited tracking", "Finding unique elements", "Set intersection/union" }),
                implementation: @"HASH SET OPERATIONS (Unique Elements)

ADD (element)
  1. Compute index = hash(element) mod capacity
  2. IF element already exists at computed location, do nothing (no duplicates)
  3. Otherwise, insert element using hash table collision handling
  Time: O(1) average

CONTAINS (element)
  1. Compute index = hash(element) mod capacity
  2. Search for element at computed location (handle collisions)
  3. Return true if found, false otherwise
  Time: O(1) average

REMOVE (element)
  1. Compute index = hash(element) mod capacity
  2. Find and remove element if present
  Time: O(1) average

SET OPERATIONS
  Union(A, B): Add all elements from B to A - O(n+m)
  Intersection(A, B): For each element in smaller set, check if in larger - O(min(n,m))
  Difference(A, B): Elements in A but not in B - O(n)",
                resources: ToJson(new object[] {
                    new { title = "Set Problems", url = "https://leetcode.com/tag/hash-table/", type = "practice" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Binary Search Tree
            DataStructure.Create(
                name: "Binary Search Tree (BST)",
                description: "Tree where left children are smaller and right children are larger than parent.",
                category: DataStructureCategory.Tree,
                whatItIs: @"A Binary Search Tree is a tree where each node has at most two children, and for every node: all values in left subtree are smaller, all values in right subtree are larger. This property enables efficient searching.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Insert", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Delete", timeComplexity = "O(h)*", description = "O(log n) balanced, O(n) worst" },
                    new { name = "Min/Max", timeComplexity = "O(h)", description = "Leftmost/rightmost node" },
                    new { name = "Inorder traversal", timeComplexity = "O(n)", description = "Visits nodes in sorted order" }
                }),
                whenToUse: @"Use BST when:
- Need ordered data with search/insert/delete
- Finding min/max efficiently
- Range queries
- Predecessor/successor queries",
                tradeoffs: @"Pros:
- Ordered data
- O(log n) operations when balanced
- Inorder gives sorted sequence

Cons:
- Can degenerate to O(n)
- More complex than hash table
- Needs balancing for guarantees",
                commonUseCases: ToJson(new[] { "Validate BST", "Kth smallest element", "LCA in BST", "BST iterator", "Range sum of BST" }),
                implementation: @"BINARY SEARCH TREE OPERATIONS

NODE STRUCTURE
  Each node contains: value, left child pointer, right child pointer
  BST Property: left subtree values < node value < right subtree values

SEARCH (root, target)
  1. IF root is null, return NOT FOUND
  2. IF target equals root.value, return root
  3. IF target < root.value, search in left subtree
  4. IF target > root.value, search in right subtree
  Time: O(h) where h is height

INSERT (root, value)
  1. IF root is null, create new node with value, return it
  2. IF value < root.value, root.left = insert(root.left, value)
  3. IF value > root.value, root.right = insert(root.right, value)
  4. Return root
  Time: O(h)

DELETE (root, value)
  1. Find node to delete using search
  2. IF leaf node: simply remove
  3. IF one child: replace with child
  4. IF two children: replace with inorder successor (min of right subtree)
  Time: O(h)

INORDER TRAVERSAL (gives sorted order)
  1. Traverse left subtree
  2. Visit current node
  3. Traverse right subtree",
                resources: ToJson(new object[] {
                    new { title = "BST - LeetCode", url = "https://leetcode.com/explore/learn/card/introduction-to-data-structure-binary-search-tree/", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Heap
            DataStructure.Create(
                name: "Heap (Priority Queue)",
                description: "Complete binary tree where parent is always smaller (min-heap) or larger (max-heap) than children.",
                category: DataStructureCategory.Heap,
                whatItIs: @"A heap is a complete binary tree satisfying the heap property: in a min-heap, every parent is ≤ its children; in a max-heap, every parent is ≥ its children. The root is always the min/max element. Usually implemented as an array.",
                operations: ToJson(new object[] {
                    new { name = "Insert", timeComplexity = "O(log n)", description = "Add and bubble up" },
                    new { name = "Extract min/max", timeComplexity = "O(log n)", description = "Remove root and heapify" },
                    new { name = "Peek min/max", timeComplexity = "O(1)", description = "View root" },
                    new { name = "Heapify array", timeComplexity = "O(n)", description = "Build heap from array" },
                    new { name = "Update priority", timeComplexity = "O(log n)", description = "Change value and reheapify" }
                }),
                whenToUse: @"Use heaps when:
- Need quick access to min or max
- Implementing priority queue
- K largest/smallest problems
- Merge k sorted lists
- Median finding",
                tradeoffs: @"Pros:
- O(1) access to min/max
- O(log n) insert/extract
- Space efficient (array)

Cons:
- Only access to min OR max
- Not sorted
- No efficient search",
                commonUseCases: ToJson(new[] { "Kth largest element", "Merge k sorted lists", "Find median", "Task scheduler", "Top k frequent" }),
                implementation: @"HEAP OPERATIONS (Array-based)

ARRAY REPRESENTATION
  Parent of index i: (i-1) / 2
  Left child of i: 2*i + 1
  Right child of i: 2*i + 2

INSERT (value)
  1. Add value at end of array
  2. BUBBLE UP: While new element < parent (min-heap):
     a. Swap with parent
     b. Move to parent index
  Time: O(log n)

EXTRACT MIN/MAX
  1. Store root value (index 0) for return
  2. Move last element to root position
  3. BUBBLE DOWN: While element > smaller child (min-heap):
     a. Swap with smaller child
     b. Move to that child's index
  4. Return stored value
  Time: O(log n)

HEAPIFY (build heap from array)
  1. Start from last non-leaf node: index (n/2 - 1)
  2. For each node from (n/2-1) down to 0:
     a. Bubble down that node
  Time: O(n) - not O(n log n) due to most nodes being near leaves

PEEK
  1. Return element at index 0
  Time: O(1)",
                resources: ToJson(new object[] {
                    new { title = "Heap - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/3", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Trie
            DataStructure.Create(
                name: "Trie (Prefix Tree)",
                description: "Tree structure for efficient string storage and prefix-based retrieval.",
                category: DataStructureCategory.Tree,
                whatItIs: @"A Trie is a tree-like data structure for storing strings where each node represents a character. The path from root to a node spells out a prefix. Enables fast prefix operations and autocomplete functionality.",
                operations: ToJson(new object[] {
                    new { name = "Insert word", timeComplexity = "O(m)", description = "m = word length" },
                    new { name = "Search word", timeComplexity = "O(m)", description = "Check if word exists" },
                    new { name = "Search prefix", timeComplexity = "O(m)", description = "Check if prefix exists" },
                    new { name = "Delete word", timeComplexity = "O(m)", description = "Remove word" },
                    new { name = "Autocomplete", timeComplexity = "O(m + k)", description = "m=prefix, k=results" }
                }),
                whenToUse: @"Use tries when:
- Prefix-based searches
- Autocomplete/typeahead
- Spell checking
- IP routing tables
- Word search in grid",
                tradeoffs: @"Pros:
- O(m) operations, independent of dictionary size
- Efficient prefix matching
- Natural for string problems

Cons:
- Memory intensive
- Slower than hash for exact lookup
- Complex implementation",
                commonUseCases: ToJson(new[] { "Implement Trie", "Word Search II", "Autocomplete", "Spell checker", "Replace words" }),
                implementation: @"TRIE (PREFIX TREE) OPERATIONS

NODE STRUCTURE
  Each node contains:
  - children: map/array of child nodes (one per character)
  - is_end_of_word: boolean flag marking complete words

INSERT (word)
  1. Start at root node
  2. For each character c in word:
     a. IF c not in current node's children, create new child node
     b. Move to child node for c
  3. Mark final node as end of word
  Time: O(m) where m = word length

SEARCH (word)
  1. Start at root node
  2. For each character c in word:
     a. IF c not in current node's children, return FALSE
     b. Move to child node for c
  3. Return TRUE only if final node is marked as end of word
  Time: O(m)

STARTS_WITH (prefix)
  1. Same as search, but return TRUE if we reach end of prefix
  2. Don't require end-of-word marker
  Time: O(m)

AUTOCOMPLETE (prefix)
  1. Navigate to node for prefix
  2. DFS/BFS from that node to collect all words
  Time: O(m + k) where k = number of results",
                resources: ToJson(new object[] {
                    new { title = "Trie - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/11", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Graph (Adjacency List)
            DataStructure.Create(
                name: "Graph (Adjacency List)",
                description: "Collection of vertices connected by edges, represented as lists of neighbors.",
                category: DataStructureCategory.Graph,
                whatItIs: @"A graph consists of vertices (nodes) and edges connecting them. The adjacency list representation stores, for each vertex, a list of its neighbors. Efficient for sparse graphs. Can be directed or undirected, weighted or unweighted.",
                operations: ToJson(new object[] {
                    new { name = "Add vertex", timeComplexity = "O(1)", description = "Create empty neighbor list" },
                    new { name = "Add edge", timeComplexity = "O(1)", description = "Add to adjacency list" },
                    new { name = "Remove edge", timeComplexity = "O(degree)", description = "Search and remove from list" },
                    new { name = "Check edge", timeComplexity = "O(degree)", description = "Search neighbor list" },
                    new { name = "Get neighbors", timeComplexity = "O(1)", description = "Return adjacency list" },
                    new { name = "DFS/BFS", timeComplexity = "O(V+E)", description = "Visit all vertices and edges" }
                }),
                whenToUse: @"Use adjacency list when:
- Graph is sparse (E << V²)
- Need to iterate over neighbors
- Memory is a concern
- Most graph algorithms",
                tradeoffs: @"Pros:
- Space efficient for sparse graphs: O(V+E)
- Fast neighbor iteration
- Easy to implement

Cons:
- O(degree) edge lookup
- Slower than matrix for dense graphs",
                commonUseCases: ToJson(new[] { "Social networks", "Road networks", "Course prerequisites", "Web crawling", "Recommendation systems" }),
                implementation: @"GRAPH (ADJACENCY LIST) OPERATIONS

REPRESENTATION
  Store a map/array where:
  - Key/index = vertex
  - Value = list of (neighbor, weight) pairs

ADD VERTEX
  1. Create new entry with empty neighbor list
  Time: O(1)

ADD EDGE (u, v, weight)
  1. Append (v, weight) to adjacency list of u
  2. For undirected: also append (u, weight) to list of v
  Time: O(1)

CHECK EDGE EXISTS (u, v)
  1. Scan adjacency list of u for v
  Time: O(degree of u)

GET NEIGHBORS (u)
  1. Return adjacency list of u
  Time: O(1)

DFS TRAVERSAL
  1. Mark starting vertex as visited
  2. For each unvisited neighbor: recursively DFS
  Time: O(V + E)

BFS TRAVERSAL
  1. Add starting vertex to queue, mark visited
  2. While queue not empty: dequeue, visit neighbors, enqueue unvisited
  Time: O(V + E)",
                resources: ToJson(new object[] {
                    new { title = "Graph - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/7", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Deque
            DataStructure.Create(
                name: "Deque (Double-ended Queue)",
                description: "Queue allowing insertion and deletion at both ends in O(1).",
                category: DataStructureCategory.Linear,
                whatItIs: @"A deque (double-ended queue) supports efficient insertion and deletion at both front and back. It combines the capabilities of both stack and queue. Python's collections.deque is implemented as a doubly-linked list.",
                operations: ToJson(new object[] {
                    new { name = "Append (right)", timeComplexity = "O(1)", description = "Add to back" },
                    new { name = "Append left", timeComplexity = "O(1)", description = "Add to front" },
                    new { name = "Pop (right)", timeComplexity = "O(1)", description = "Remove from back" },
                    new { name = "Pop left", timeComplexity = "O(1)", description = "Remove from front" },
                    new { name = "Access ends", timeComplexity = "O(1)", description = "Peek front/back" },
                    new { name = "Access by index", timeComplexity = "O(n)", description = "Random access slow" }
                }),
                whenToUse: @"Use deque when:
- Need O(1) operations at both ends
- Implementing sliding window
- BFS (more efficient than list)
- Need both stack and queue operations",
                tradeoffs: @"Pros:
- O(1) at both ends
- More flexible than stack or queue
- Good for sliding window

Cons:
- O(n) random access
- More memory than array
- Slightly complex API",
                commonUseCases: ToJson(new[] { "Sliding window maximum", "BFS implementation", "Palindrome checker", "Task scheduling", "Recent calls" }),
                implementation: @"DEQUE (DOUBLE-ENDED QUEUE) OPERATIONS

REPRESENTATION
  Typically implemented as doubly-linked list or circular buffer

ADD TO BACK (value)
  1. Create new node with value
  2. Link current tail to new node
  3. Update tail pointer
  Time: O(1)

ADD TO FRONT (value)
  1. Create new node with value
  2. Link new node to current head
  3. Update head pointer
  Time: O(1)

REMOVE FROM BACK
  1. Store tail value for return
  2. Update tail to previous node
  3. Return stored value
  Time: O(1)

REMOVE FROM FRONT
  1. Store head value for return
  2. Update head to next node
  3. Return stored value
  Time: O(1)

USE CASE: SLIDING WINDOW MAXIMUM
  - Add indices to back as you traverse
  - Remove from front when outside window
  - Remove from back when current value is larger
  - Front always contains index of maximum in window",
                resources: ToJson(new object[] {
                    new { title = "Deque Documentation", url = "https://docs.python.org/3/library/collections.html#collections.deque", type = "documentation" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Union-Find
            DataStructure.Create(
                name: "Union-Find (Disjoint Set)",
                description: "Data structure for tracking disjoint sets with near O(1) union and find operations.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"Union-Find (Disjoint Set Union) maintains a collection of non-overlapping sets. Supports finding which set an element belongs to and merging sets. With path compression and union by rank, operations are nearly O(1) amortized.",
                operations: ToJson(new object[] {
                    new { name = "Find", timeComplexity = "O(α(n))", description = "Find set representative (nearly O(1))" },
                    new { name = "Union", timeComplexity = "O(α(n))", description = "Merge two sets" },
                    new { name = "Connected", timeComplexity = "O(α(n))", description = "Check if same set" },
                    new { name = "Count sets", timeComplexity = "O(1)", description = "Number of disjoint sets" }
                }),
                whenToUse: @"Use Union-Find when:
- Tracking connected components
- Dynamic connectivity
- Kruskal's MST
- Detecting cycles in undirected graph
- Grouping equivalent items",
                tradeoffs: @"Pros:
- Nearly O(1) operations
- Efficient for connectivity queries
- Simple to implement

Cons:
- Only tracks connectivity, not paths
- Cannot efficiently split sets
- Amortized not worst-case",
                commonUseCases: ToJson(new[] { "Number of connected components", "Redundant connection", "Accounts merge", "Making a large island", "Graph valid tree" }),
                implementation: @"UNION-FIND (DISJOINT SET) OPERATIONS

DATA STRUCTURE
  - parent[i]: parent of element i (initially itself)
  - rank[i]: approximate depth of tree rooted at i
  - count: number of disjoint sets

FIND (x) - with path compression
  1. IF parent[x] equals x, return x (x is root)
  2. Otherwise, recursively find root: root = find(parent[x])
  3. PATH COMPRESSION: set parent[x] = root (flatten tree)
  4. Return root
  Time: O(α(n)) ≈ O(1) amortized

UNION (x, y) - with union by rank
  1. Find roots: rootX = find(x), rootY = find(y)
  2. IF rootX equals rootY, already in same set - return false
  3. UNION BY RANK: attach smaller tree under larger tree
     - IF rank[rootX] < rank[rootY], swap them
     - Set parent[rootY] = rootX
     - IF ranks were equal, increment rank[rootX]
  4. Decrement component count
  5. Return true
  Time: O(α(n)) ≈ O(1) amortized

CONNECTED (x, y)
  1. Return find(x) equals find(y)
  Time: O(α(n))",
                resources: ToJson(new object[] {
                    new { title = "Union Find - NeetCode", url = "https://neetcode.io/courses/advanced-algorithms/9", type = "course" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Segment Tree
            DataStructure.Create(
                name: "Segment Tree",
                description: "Tree structure for efficient range queries and updates on arrays.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Segment Tree is a binary tree where each node represents an interval of the array. The root represents the entire array, and each leaf represents a single element. Internal nodes store aggregate information (sum, min, max) of their children's intervals. Enables O(log n) range queries and updates.",
                operations: ToJson(new object[] {
                    new { name = "Build", timeComplexity = "O(n)", description = "Construct tree from array" },
                    new { name = "Range query", timeComplexity = "O(log n)", description = "Query sum/min/max over range" },
                    new { name = "Point update", timeComplexity = "O(log n)", description = "Update single element" },
                    new { name = "Range update", timeComplexity = "O(log n)", description = "With lazy propagation" }
                }),
                whenToUse: @"Use Segment Tree when:
- Need range queries (sum, min, max, GCD)
- Need point or range updates
- Multiple queries on same array
- Competitive programming",
                tradeoffs: @"Pros:
- O(log n) range queries and updates
- Flexible aggregate functions
- Supports lazy propagation

Cons:
- O(n) space (typically 4n)
- Complex implementation
- Overkill for simple problems",
                commonUseCases: ToJson(new[] { "Range Sum Query - Mutable", "Range Minimum Query", "Count of Smaller Numbers After Self", "Falling Squares", "Rectangle Area II" }),
                implementation: @"SEGMENT TREE OPERATIONS

STRUCTURE (Array-based)
  - tree[0] = root (entire array aggregate)
  - tree[2*i+1] = left child of tree[i]
  - tree[2*i+2] = right child of tree[i]
  - Leaves store individual array elements
  - Internal nodes store aggregate (sum/min/max) of children

BUILD (array, node, start, end)
  1. IF start equals end (leaf): tree[node] = array[start]
  2. ELSE:
     a. mid = (start + end) / 2
     b. Build left child: build(array, 2*node+1, start, mid)
     c. Build right child: build(array, 2*node+2, mid+1, end)
     d. tree[node] = aggregate(left child, right child)
  Time: O(n)

POINT UPDATE (node, start, end, index, value)
  1. IF leaf (start equals end): update tree[node]
  2. ELSE:
     a. mid = (start + end) / 2
     b. IF index <= mid: recurse on left child
     c. ELSE: recurse on right child
     d. Recompute tree[node] from children
  Time: O(log n)

RANGE QUERY (node, start, end, queryLeft, queryRight)
  1. IF no overlap (queryRight < start OR queryLeft > end): return identity
  2. IF total overlap (queryLeft <= start AND end <= queryRight): return tree[node]
  3. ELSE: return aggregate of queries on both children
  Time: O(log n)",
                resources: ToJson(new object[] {
                    new { title = "Segment Tree - CP Algorithms", url = "https://cp-algorithms.com/data_structures/segment_tree.html", type = "article" },
                    new { title = "Segment Tree Tutorial", url = "https://www.geeksforgeeks.org/segment-tree-data-structure/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Fenwick Tree (Binary Indexed Tree)
            DataStructure.Create(
                name: "Fenwick Tree (Binary Indexed Tree)",
                description: "Space-efficient structure for prefix sums with O(log n) updates and queries.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Fenwick Tree (Binary Indexed Tree or BIT) efficiently computes prefix sums and supports point updates. Uses clever bit manipulation to determine parent-child relationships. More space-efficient than Segment Tree (uses n elements vs 4n).",
                operations: ToJson(new object[] {
                    new { name = "Build", timeComplexity = "O(n log n)", description = "Initialize from array" },
                    new { name = "Point update", timeComplexity = "O(log n)", description = "Add value at index" },
                    new { name = "Prefix sum", timeComplexity = "O(log n)", description = "Sum from 0 to index" },
                    new { name = "Range sum", timeComplexity = "O(log n)", description = "Sum from i to j" }
                }),
                whenToUse: @"Use Fenwick Tree when:
- Need prefix sums with updates
- Counting inversions
- Range sum queries
- Simpler than Segment Tree suffices",
                tradeoffs: @"Pros:
- O(n) space (more efficient than Segment Tree)
- Simple implementation
- Fast in practice

Cons:
- Less flexible than Segment Tree
- Only works for associative, invertible operations
- Cannot do range updates easily",
                commonUseCases: ToJson(new[] { "Range Sum Query - Mutable", "Count of Smaller Numbers After Self", "Count inversions", "2D range sums" }),
                implementation: @"FENWICK TREE (BIT) OPERATIONS

KEY INSIGHT
  - Uses binary representation of indices
  - i & (-i) gives the lowest set bit (LSB)
  - Each index i is responsible for a range of size LSB(i)

STRUCTURE (1-indexed array)
  - tree[i] stores sum of elements in range that ends at i
  - Range size determined by lowest set bit of i

UPDATE (index, delta)
  1. WHILE index <= n:
     a. Add delta to tree[index]
     b. Move to next responsible index: index += (index & -index)
  Time: O(log n)

PREFIX SUM (index) - sum from 1 to index
  1. Initialize total = 0
  2. WHILE index > 0:
     a. Add tree[index] to total
     b. Remove lowest set bit: index -= (index & -index)
  3. Return total
  Time: O(log n)

RANGE SUM (left, right)
  1. Return prefixSum(right) - prefixSum(left - 1)
  Time: O(log n)

BIT MANIPULATION TRICK
  - (i & -i) isolates the lowest set bit
  - Example: 12 (1100) & -12 (0100) = 4",
                resources: ToJson(new object[] {
                    new { title = "Fenwick Tree - CP Algorithms", url = "https://cp-algorithms.com/data_structures/fenwick.html", type = "article" },
                    new { title = "BIT Tutorial", url = "https://www.topcoder.com/thrive/articles/Binary%20Indexed%20Trees", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // AVL Tree
            DataStructure.Create(
                name: "AVL Tree",
                description: "Self-balancing BST maintaining height balance for guaranteed O(log n) operations.",
                category: DataStructureCategory.Tree,
                whatItIs: @"An AVL Tree is a self-balancing Binary Search Tree where the heights of left and right subtrees differ by at most 1 for every node. Named after Adelson-Velsky and Landis. Uses rotations to maintain balance after insertions and deletions.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(log n)", description = "Guaranteed balanced height" },
                    new { name = "Insert", timeComplexity = "O(log n)", description = "Insert + rebalance" },
                    new { name = "Delete", timeComplexity = "O(log n)", description = "Delete + rebalance" },
                    new { name = "Min/Max", timeComplexity = "O(log n)", description = "Leftmost/rightmost" }
                }),
                whenToUse: @"Use AVL Tree when:
- Need guaranteed O(log n) operations
- Frequent searches (more balanced than Red-Black)
- Ordered data with strict time bounds",
                tradeoffs: @"Pros:
- Strictly balanced (height ≤ 1.44 log n)
- Faster lookups than Red-Black Tree
- Guaranteed O(log n) worst case

Cons:
- More rotations on insert/delete
- Slower insertions than Red-Black
- Extra height storage per node",
                commonUseCases: ToJson(new[] { "Database indexing", "In-memory sorted data", "Interval trees", "Priority scheduling" }),
                implementation: @"AVL TREE OPERATIONS (Self-Balancing BST)

BALANCE PROPERTY
  For every node: |height(left subtree) - height(right subtree)| ≤ 1
  Balance factor = height(left) - height(right)

ROTATIONS (to restore balance)
  RIGHT ROTATE (when left-heavy):
    1. y becomes the root
    2. x (y's left child) becomes new root
    3. x's right subtree becomes y's left subtree
    4. y becomes x's right child
    5. Update heights

  LEFT ROTATE (when right-heavy):
    Mirror of right rotation

INSERT (value)
  1. Perform standard BST insert
  2. Update height of current node
  3. Calculate balance factor
  4. IF unbalanced, apply appropriate rotation:
     - Left-Left (balance > 1, value < left.value): right rotate
     - Right-Right (balance < -1, value > right.value): left rotate
     - Left-Right (balance > 1, value > left.value): left rotate left child, then right rotate
     - Right-Left (balance < -1, value < right.value): right rotate right child, then left rotate
  Time: O(log n)

DELETE (value)
  1. Perform standard BST delete
  2. Update heights back up to root
  3. Rebalance each ancestor if needed
  Time: O(log n)",
                resources: ToJson(new object[] {
                    new { title = "AVL Tree - GeeksforGeeks", url = "https://www.geeksforgeeks.org/introduction-to-avl-tree/", type = "article" },
                    new { title = "AVL Tree Visualization", url = "https://www.cs.usfca.edu/~galles/visualization/AVLtree.html", type = "visualization" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // LRU Cache
            DataStructure.Create(
                name: "LRU Cache",
                description: "Cache with Least Recently Used eviction policy using hash map and doubly linked list.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"An LRU (Least Recently Used) Cache is a data structure that stores a limited number of items and evicts the least recently accessed item when full. Combines a hash map for O(1) lookup with a doubly linked list for O(1) recency tracking.",
                operations: ToJson(new object[] {
                    new { name = "Get", timeComplexity = "O(1)", description = "Retrieve value and mark as recently used" },
                    new { name = "Put", timeComplexity = "O(1)", description = "Insert/update and evict if full" },
                    new { name = "Delete", timeComplexity = "O(1)", description = "Remove specific key" }
                }),
                whenToUse: @"Use LRU Cache when:
- Need caching with size limit
- Recently used items are more valuable
- Implementing memoization
- Database/web caching",
                tradeoffs: @"Pros:
- O(1) all operations
- Automatic eviction
- Good cache hit rates for temporal locality

Cons:
- Extra memory for linked list
- Not optimal for all access patterns
- More complex than simple cache",
                commonUseCases: ToJson(new[] { "LRU Cache problem", "Web browser cache", "Database buffer pool", "CPU cache simulation", "API response caching" }),
                implementation: @"LRU CACHE OPERATIONS

DATA STRUCTURE
  - Hash map: key -> node reference (O(1) lookup)
  - Doubly linked list: maintains recency order
    - Head = most recently used
    - Tail = least recently used
  - Use dummy head/tail nodes to simplify edge cases

GET (key)
  1. IF key not in hash map: return NOT_FOUND
  2. Get node from hash map
  3. Move node to head of linked list (mark as recently used)
  4. Return node's value
  Time: O(1)

PUT (key, value)
  1. IF key exists in hash map:
     a. Update node's value
     b. Move node to head
  2. ELSE (new key):
     a. Create new node with key-value
     b. Add to hash map
     c. Add node to head of list
     d. IF size > capacity:
        - Remove tail node (least recently used)
        - Remove from hash map
  Time: O(1)

HELPER: Move to Head
  1. Remove node from current position (update prev/next pointers)
  2. Insert node right after dummy head

HELPER: Remove Tail
  1. Get tail.prev (actual last node)
  2. Remove it from list
  3. Return its key (for hash map removal)",
                resources: ToJson(new object[] {
                    new { title = "LRU Cache - LeetCode", url = "https://leetcode.com/problems/lru-cache/", type = "problem" },
                    new { title = "LRU Cache Design", url = "https://www.geeksforgeeks.org/lru-cache-implementation/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Skip List
            DataStructure.Create(
                name: "Skip List",
                description: "Probabilistic data structure with O(log n) search using multiple linked list layers.",
                category: DataStructureCategory.Advanced,
                whatItIs: @"A Skip List is a probabilistic data structure that uses multiple layers of linked lists to achieve O(log n) average search time. Each element has a random height determining which layers it appears in. Higher layers act as 'express lanes' for faster traversal.",
                operations: ToJson(new object[] {
                    new { name = "Search", timeComplexity = "O(log n)*", description = "Average case, O(n) worst" },
                    new { name = "Insert", timeComplexity = "O(log n)*", description = "Average case" },
                    new { name = "Delete", timeComplexity = "O(log n)*", description = "Average case" },
                    new { name = "Range query", timeComplexity = "O(log n + k)", description = "k = number of results" }
                }),
                whenToUse: @"Use Skip List when:
- Need ordered data with fast operations
- Simpler alternative to balanced trees
- Concurrent access needed (lock-free variants)
- Range queries required",
                tradeoffs: @"Pros:
- Simple to implement vs balanced trees
- Good for concurrent access
- No rotations needed
- Efficient range queries

Cons:
- Probabilistic guarantees (not worst-case)
- More space than BST
- Cache-unfriendly",
                commonUseCases: ToJson(new[] { "Redis sorted sets", "LevelDB/RocksDB", "Concurrent data structures", "In-memory indexes" }),
                implementation: @"SKIP LIST OPERATIONS

STRUCTURE
  Each node contains:
    - value: The stored data
    - forward[]: Array of pointers to next nodes at each level

  Head node: Sentinel with value -∞ and max_level pointers
  Parameters: max_level (typically 16), probability p (typically 0.5)

RANDOM_LEVEL ()
  1. Start with level = 0
  2. While random() < p AND level < max_level:
     a. Increment level
  3. Return level
  Note: Creates geometric distribution of levels
  Time: O(log n) expected

SEARCH (target)
  1. Start at head, level = current max level
  2. For each level from top to bottom:
     a. While forward[level] exists AND forward[level].value < target:
        - Move forward: current = current.forward[level]
     b. Drop down one level
  3. Move to forward[0] (bottom level)
  4. Return true if current.value == target
  Time: O(log n) expected

INSERT (value)
  1. Create update[] array to track predecessors at each level
  2. Search path (like SEARCH), recording predecessor at each level:
     For each level from top to bottom:
       While forward[level] exists AND forward[level].value < value:
         Move forward
       update[level] = current position
  3. Generate random level for new node
  4. If new level > current max level:
     a. For levels (max_level+1) to new_level:
        update[level] = head
     b. Update max level
  5. Create new node with value and level
  6. For each level 0 to new_level:
     a. new_node.forward[level] = update[level].forward[level]
     b. update[level].forward[level] = new_node
  Time: O(log n) expected

DELETE (value)
  1. Search and record update[] array
  2. If target found at bottom level:
     a. For each level where node appears:
        update[level].forward[level] = node.forward[level]
     b. While max_level > 0 AND head.forward[max_level] is null:
        Decrement max_level
  Time: O(log n) expected",
                resources: ToJson(new object[] {
                    new { title = "Skip List - Wikipedia", url = "https://en.wikipedia.org/wiki/Skip_list", type = "article" },
                    new { title = "Skip List Tutorial", url = "https://www.geeksforgeeks.org/skip-list/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Monotonic Queue
            DataStructure.Create(
                name: "Monotonic Queue",
                description: "Queue maintaining elements in sorted order for efficient sliding window min/max.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A Monotonic Queue (or Monotonic Deque) maintains elements in increasing or decreasing order. When adding an element, removes all elements that violate the monotonic property. Combined with index tracking, enables O(1) amortized sliding window min/max queries.",
                operations: ToJson(new object[] {
                    new { name = "Push", timeComplexity = "O(1)*", description = "Amortized, may remove multiple elements" },
                    new { name = "Pop front", timeComplexity = "O(1)", description = "Remove expired elements" },
                    new { name = "Get min/max", timeComplexity = "O(1)", description = "Front of queue" }
                }),
                whenToUse: @"Use Monotonic Queue when:
- Sliding window minimum/maximum
- Need to maintain sorted window
- Next greater/smaller with window",
                tradeoffs: @"Pros:
- O(1) amortized operations
- Perfect for sliding window problems
- Each element pushed/popped once

Cons:
- Specific use case
- More complex than simple queue
- Only maintains one extreme",
                commonUseCases: ToJson(new[] { "Sliding Window Maximum", "Shortest Subarray with Sum at Least K", "Jump Game VI", "Constrained Subsequence Sum" }),
                implementation: @"MONOTONIC QUEUE OPERATIONS (for Maximum queries)

STRUCTURE
  Deque storing (value, index) pairs
  Maintains DECREASING order from front to back
  Front always contains current maximum

  For MINIMUM queries: maintain INCREASING order instead

PUSH (value, index)
  1. While deque not empty AND back element's value <= value:
     a. Remove from back (these can never be max while new element exists)
  2. Add (value, index) to back of deque
  Time: O(1) amortized (each element pushed/popped at most once)

POP_EXPIRED (left_bound)
  1. While deque not empty AND front element's index < left_bound:
     a. Remove from front (element is outside current window)
  Time: O(1) amortized

GET_MAX ()
  1. Return front element's value (or null if empty)
  Time: O(1)

---

SLIDING WINDOW MAXIMUM ALGORITHM

Input: array nums[], window size k
Output: array of maximum values for each window position

1. Initialize empty monotonic queue
2. Initialize empty result array
3. For i = 0 to length(nums) - 1:
   a. PUSH(nums[i], i) into monotonic queue
   b. POP_EXPIRED(i - k + 1) to remove elements outside window
   c. If i >= k - 1 (window is full):
      - Append GET_MAX() to result
4. Return result

Time: O(n) - each element pushed and popped at most once
Space: O(k) - deque holds at most k elements

KEY INSIGHT
  When a larger element enters, smaller elements before it
  can NEVER be the maximum while the larger element is in
  the window. So we can safely discard them.",
                resources: ToJson(new object[] {
                    new { title = "Sliding Window Maximum", url = "https://leetcode.com/problems/sliding-window-maximum/", type = "problem" },
                    new { title = "Monotonic Queue Explained", url = "https://www.geeksforgeeks.org/sliding-window-maximum-maximum-of-all-subarrays-of-size-k/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Graph (Adjacency Matrix)
            DataStructure.Create(
                name: "Graph (Adjacency Matrix)",
                description: "Graph represented as 2D matrix where matrix[i][j] indicates edge between vertices i and j.",
                category: DataStructureCategory.Graph,
                whatItIs: @"An Adjacency Matrix represents a graph as a 2D array where entry [i][j] indicates if there's an edge from vertex i to j (and its weight for weighted graphs). O(1) edge lookup but O(V²) space regardless of edge count.",
                operations: ToJson(new object[] {
                    new { name = "Check edge", timeComplexity = "O(1)", description = "Direct array access" },
                    new { name = "Add edge", timeComplexity = "O(1)", description = "Set matrix entry" },
                    new { name = "Remove edge", timeComplexity = "O(1)", description = "Clear matrix entry" },
                    new { name = "Get all neighbors", timeComplexity = "O(V)", description = "Scan entire row" },
                    new { name = "Space", timeComplexity = "O(V²)", description = "Fixed regardless of edges" }
                }),
                whenToUse: @"Use Adjacency Matrix when:
- Graph is dense (E ≈ V²)
- Need O(1) edge existence checks
- Working with weighted graphs
- Floyd-Warshall or similar algorithms",
                tradeoffs: @"Pros:
- O(1) edge lookup
- Simple implementation
- Good for dense graphs
- Easy to represent weighted edges

Cons:
- O(V²) space always
- Wasteful for sparse graphs
- O(V) to find all neighbors",
                commonUseCases: ToJson(new[] { "Floyd-Warshall", "Dense graph algorithms", "Small graphs", "Graph with many edge queries" }),
                implementation: @"ADJACENCY MATRIX OPERATIONS

STRUCTURE
  2D array matrix[V][V] where V = number of vertices
  matrix[i][j] = 1 if edge from i to j exists (unweighted)
  matrix[i][j] = weight if edge exists (weighted)
  matrix[i][j] = 0 or ∞ if no edge

INITIALIZE_UNWEIGHTED (n, edges[], directed)
  1. Create n × n matrix, fill with 0
  2. For each edge (u, v) in edges:
     a. matrix[u][v] = 1
     b. If undirected: matrix[v][u] = 1
  Time: O(V² + E)
  Space: O(V²)

INITIALIZE_WEIGHTED (n, edges[], directed)
  1. Create n × n matrix, fill with ∞
  2. For i = 0 to n-1:
     a. matrix[i][i] = 0  (distance to self)
  3. For each edge (u, v, weight) in edges:
     a. matrix[u][v] = weight
     b. If undirected: matrix[v][u] = weight
  Time: O(V² + E)
  Space: O(V²)

HAS_EDGE (u, v)
  1. Return matrix[u][v] ≠ 0 (unweighted)
     OR matrix[u][v] ≠ ∞ (weighted)
  Time: O(1)

ADD_EDGE (u, v, weight=1, directed=false)
  1. matrix[u][v] = weight
  2. If undirected: matrix[v][u] = weight
  Time: O(1)

REMOVE_EDGE (u, v, directed=false)
  1. matrix[u][v] = 0 (or ∞ for weighted)
  2. If undirected: matrix[v][u] = 0 (or ∞)
  Time: O(1)

GET_NEIGHBORS (u)
  1. Initialize empty list neighbors
  2. For v = 0 to V-1:
     a. If matrix[u][v] ≠ 0 (or ≠ ∞):
        Add v to neighbors
  3. Return neighbors
  Time: O(V)

GET_EDGE_WEIGHT (u, v)
  1. Return matrix[u][v]
  Time: O(1)

BEST FOR: Dense graphs, Floyd-Warshall, frequent edge queries",
                resources: ToJson(new object[] {
                    new { title = "Graph Representations", url = "https://www.geeksforgeeks.org/graph-and-its-representations/", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            ),

            // Circular Buffer
            DataStructure.Create(
                name: "Circular Buffer (Ring Buffer)",
                description: "Fixed-size buffer that wraps around, useful for streaming data and producer-consumer patterns.",
                category: DataStructureCategory.Linear,
                whatItIs: @"A Circular Buffer (Ring Buffer) is a fixed-size queue that wraps around when reaching the end. Uses two pointers (head and tail) with modulo arithmetic. When full, new data overwrites the oldest data. Efficient for streaming and bounded queues.",
                operations: ToJson(new object[] {
                    new { name = "Enqueue", timeComplexity = "O(1)", description = "Add to tail" },
                    new { name = "Dequeue", timeComplexity = "O(1)", description = "Remove from head" },
                    new { name = "Peek", timeComplexity = "O(1)", description = "View head element" },
                    new { name = "IsFull/IsEmpty", timeComplexity = "O(1)", description = "Check buffer state" }
                }),
                whenToUse: @"Use Circular Buffer when:
- Fixed memory budget
- Streaming/real-time data
- Producer-consumer patterns
- Audio/video buffering
- Logging recent events",
                tradeoffs: @"Pros:
- Fixed memory usage
- O(1) all operations
- No memory allocation after init
- Cache-friendly

Cons:
- Fixed capacity
- Loses old data when full
- Slightly complex pointer management",
                commonUseCases: ToJson(new[] { "Design Circular Queue", "Moving average from data stream", "Audio buffers", "Network packet buffers", "Log rotation" }),
                implementation: @"CIRCULAR BUFFER (RING BUFFER) OPERATIONS

STRUCTURE
  Fixed-size array buffer[capacity]
  head: Index for reading (dequeue position)
  tail: Index for writing (enqueue position)
  size: Current number of elements

  Visual (capacity=5, 3 elements):
  [_, A, B, C, _]
      ^head   ^tail

INITIALIZE (capacity)
  1. Create array of size capacity
  2. head = 0
  3. tail = 0
  4. size = 0
  Time: O(capacity)

IS_EMPTY ()
  1. Return size == 0
  Time: O(1)

IS_FULL ()
  1. Return size == capacity
  Time: O(1)

ENQUEUE (value)
  1. If IS_FULL():
     a. Return false (or for overwrite mode: advance head)
  2. buffer[tail] = value
  3. tail = (tail + 1) % capacity    // Wrap around!
  4. size = size + 1
  5. Return true
  Time: O(1)

DEQUEUE ()
  1. If IS_EMPTY(): Return null/error
  2. value = buffer[head]
  3. head = (head + 1) % capacity    // Wrap around!
  4. size = size - 1
  5. Return value
  Time: O(1)

FRONT (peek front)
  1. If IS_EMPTY(): Return null
  2. Return buffer[head]
  Time: O(1)

REAR (peek back)
  1. If IS_EMPTY(): Return null
  2. Return buffer[(tail - 1 + capacity) % capacity]
     Note: Add capacity before mod to handle negative
  Time: O(1)

KEY INSIGHT: MODULO ARITHMETIC
  The % operator creates the ""wrap around"" behavior:
  - If tail reaches end, (tail + 1) % capacity wraps to 0
  - Buffer acts like a circle, not a line
  - No shifting needed, O(1) operations",
                resources: ToJson(new object[] {
                    new { title = "Design Circular Queue - LeetCode", url = "https://leetcode.com/problems/design-circular-queue/", type = "problem" },
                    new { title = "Circular Buffer - Wikipedia", url = "https://en.wikipedia.org/wiki/Circular_buffer", type = "article" }
                }),
                relatedStructureIds: ToJson(Array.Empty<Guid>())
            )
        ];
    }
}
