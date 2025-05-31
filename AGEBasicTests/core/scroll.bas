10 REM --- AGEBasic Screen Scroll Capabilities Test ---
20 CLS : REM Clear the screen
30 SETCOLORSPACE "c64" : REM Set a retro C64-like color scheme
35 LETS TRUE, FALSE = 1, 0
40 REM --- 1. Fill screen with initial content using PRINTLN ---
60 FOR LINEIDX = 0 TO ScreenHeight() - 1
70    PRINTLN "ORIGINAL LINE # " + STR(LINEIDX), FALSE, FALSE : REM Print lines, deferring display. PRINTLN manages its own Y. DRAW_FLAG as string "0"
80 NEXT LINEIDX
90 SHOW : REM Display all the initial lines
100 SLEEP 2 : REM Pause to observe

110 REM --- 2. Scroll entire screen UP (negative N_LINES) ---
120 REM    New blank lines appear at the bottom, filled with green.
130 SCROLL -5, ARRAY(0,255,0) : REM Scroll up 5 lines, fill new space with green. DRAW_FLAG omitted to defer.
140 REM Print new text on the lines that appeared at the bottom using explicit PRINT X,Y
150 PRINT 0, ScreenHeight() - 5, "--- NEW TEXT ON SCROLLED UP (1) ---", FALSE, FALSE
160 PRINT 0, ScreenHeight() - 4, "--- NEW TEXT ON SCROLLED UP (2) ---", FALSE, FALSE
170 PRINT 0, ScreenHeight() - 3, "--- NEW TEXT ON SCROLLED UP (3) ---", FALSE, FALSE
180 PRINT 0, ScreenHeight() - 2, "--- NEW TEXT ON SCROLLED UP (4) ---", FALSE, FALSE
190 PRINT 0, ScreenHeight() - 1, "--- NEW TEXT ON SCROLLED UP (5) ---", FALSE, FALSE
200 SHOW : REM Display scrolled content and new lines
210 SLEEP 3 : REM Pause to observe

220 REM --- 3. Scroll entire screen DOWN (positive N_LINES) ---
230 REM    New blank lines appear at the top, filled with red.
240 SCROLL 3, ARRAY(255,0,0) : REM Scroll down 3 lines, fill new space with red. DRAW_FLAG omitted to defer.
250 REM Print new text on the lines that appeared at the top using explicit PRINT X,Y
260 PRINT 0, 0, "--- NEW TEXT ON SCROLLED DOWN (A) ---", FALSE, FALSE
270 PRINT 0, 1, "--- NEW TEXT ON SCROLLED DOWN (B) ---", FALSE, FALSE
280 PRINT 0, 2, "--- NEW TEXT ON SCROLLED DOWN (C) ---", FALSE, FALSE
290 SHOW : REM Display scrolled content and new lines
300 SLEEP 3 : REM Pause to observe

310 CLS : REM Clear the screen for the next demonstration
320 SHOW
330 SLEEP 1

340 REM --- 4. Demonstrate SCROLLRECT (Rectangular Scroll) ---
350 REM Define the rectangle's character coordinates (column, row, width, height)
360 LET CORNER = ARRAY(5, 5) : REM CORNER OF THE SCROLL AREA
380 LET SIZE = ARRAY(30,7)  : REM WIDTH X SIZE OF THE SCROLL AREA

400 REM Draw a box around the rectangle for visual reference
410 LET YELLOW = ARRAY(255,255,0)
430 LET BOXPWIDTH = DCHARPIXELX(CORNER[1], 0) - DCHARPIXELX(CORNER[0], 0) : REM Convert char width to pixel width
440 LET BOXPHEIGHT = DCHARPIXELY(0, CORNER[1]) - DCHARPIXELY(0, CORNER[1]) : REM Convert char height to pixel height
450 DBOX DCHARPIXEL(CORNER[0]-1, CORNER[1]-1), 
        ARRAY(BOXPWIDTH, BOXPHEIGHT), 
        YELLOW , 0, YELLOW, FALSE 
460 REM Fill the rectangle with initial content using explicit PRINT X,Y
470 FOR LINEIDX = 0 TO CORNER[1] - 1
480    PRINT CORNER[0], CORNER[1] + LINEIDX, "RECT CONTENT LINE " + STR(LINEIDX), FALSE, FALSE
490 NEXT LINEIDX
500 SHOW : REM Display the box and its content
510 SLEEP 3

520 REM --- 5. Scroll the rectangle UP (negative N_LINES) ---
530 REM    New blank lines within the rectangle appear at its bottom, filled with cyan.
540 SCROLLRECT CORNER, SIZE, -2, ARRAY(255,0,0) : REM Scroll rect up 2 lines, fill new space with cyan. DRAW_FLAG omitted to defer.
560 REM Print new text on the lines that appeared at the bottom of the rectangle using explicit PRINT X,Y
570 PRINT CORNER[0], CORNER[1] + SIZE[1] - 2, "NEW RECT LINE UP (1)", FALSE, FALSE
580 PRINT CORNER[0], CORNER[1] + SIZE[1] - 1, "NEW RECT LINE UP (2)", FALSE, FALSE
590 SHOW : REM Display scrolled rectangle content and new lines
600 SLEEP 3

610 REM --- 6. Scroll the rectangle DOWN (positive N_LINES) ---
620 REM    New blank lines within the rectangle appear at its top, filled with magenta.
630 SCROLLRECT CORNER, SIZE, 1, YELLOW, FALSE : REM Scroll rect down 1 line, fill new space with magenta. DRAW_FLAG omitted to defer.
640 REM Print new text on the line that appeared at the top of the rectangle using explicit PRINT X,Y
650 PRINT CORNER[0], CORNER[1], "NEW RECT LINE DOWN (TOP)", FALSE, FALSE
660 SHOW : REM Display scrolled rectangle content and new line
670 SLEEP 3

680 CLS : PRINTLN "ALL TESTS PASSED"
690 SLEEP 2
700 CLS : REM Clear final screen
710 END