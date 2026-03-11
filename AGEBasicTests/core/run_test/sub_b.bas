10 REM Sub-program B in the A -> B -> C chain
20 LET CHAIN_VAR = CHAIN_VAR + 1
30 RUN "sub_c.bas"
40 LET B_FINISHED = 1
50 END