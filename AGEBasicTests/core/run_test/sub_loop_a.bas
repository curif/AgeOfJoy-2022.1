10 REM Endless loop test (A -> B -> A) Part A
20 LET NEST_DEPTH = NEST_DEPTH + 1
30 IF NEST_DEPTH > 22 THEN LET ERROR = "Failed to throw stack overflow exception!" : END
40 RUN "sub_loop_b.bas"
50 END