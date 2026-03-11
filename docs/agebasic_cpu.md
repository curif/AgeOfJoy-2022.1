# AGEBasic: Optimizing Performance with SETCPU

By default, AGEBasic scripts run at a legacy-compatible speed of **1 line per frame**. On a Meta Quest running at 72 FPS, this results in 72 lines of code executed per second. 

While this is perfect for simple UI and backward compatibility, modern cabinets or complex calculations may require much higher performance. The `SETCPU` command allows you to "overclock" your scripts.

---

## The SETCPU Command

The `SETCPU` command directly controls how many lines of AGEBASIC code the interpreter attempts to execute in a single Unity frame.

### Syntax
```basic
SETCPU <multiplier>
```

*   **`<multiplier>`**: A positive number representing lines-per-frame.
*   **Default**: If not called, the multiplier is `1`.

### Common Values
| Value | Speed (at 72 FPS) | Best Use Case |
| :--- | :--- | :--- |
| `SETCPU 1` | ~72 lines/sec | Legacy scripts, slow animations, simple menus. |
| `SETCPU 10` | ~720 lines/sec | Responsive UI, smooth object movement. |
| `SETCPU 100` | ~7,200 lines/sec | Complex math, string processing, data sorting. |
| `SETCPU 500` | ~36,000 lines/sec | Maximum performance for heavy background tasks. |

---

## Example Usage

```basic
10 SETCPU 100 ' Overclock to 100 lines per frame
20 FOR I = 1 TO 1000
30   PRINT 0, 0, "Processing item: " + STR(I)
40 NEXT I
50 SETCPU 1 ' Return to normal speed for the rest of the script
```

---

## Safety & FPS Protection

You can set `SETCPU` to very high values (e.g., `SETCPU 5000`) without crashing the game. AGEBasic includes a built-in **FPS Guard** (typically 2.0ms). 

1.  The interpreter starts the batch of lines.
2.  If it finishes the batch, it yields to the next frame.
3.  If the batch is taking too long and reaches the **2.0ms time limit**, it will automatically pause and resume in the next frame.

This ensures that even a "runaway" high-speed script will **never cause a frame drop** in VR.

## Best Practices

1.  **Use it where needed:** Only increase the CPU multiplier for heavy blocks of code. Keeping it at `1` for simple menus saves battery life and reduces overall CPU heat on standalone headsets.
2.  **SLEEP yields priority:** If your script uses the `SLEEP` command, it will always pause for the requested duration regardless of the `SETCPU` setting.
3.  **Global vs Local:** `SETCPU` affects the currently running program. If you have multiple cabinets in a room, each cabinet's script manages its own speed independently.
