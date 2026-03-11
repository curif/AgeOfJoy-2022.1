5 LET ERROR = ""
10 PRINTLN "--- TEST RUN COMMAND SUITE ---"

20 REM 1. Test Simple RUN
30 LET RUN_TEST_1_OK = 0
40 RUN "run_test/sub_simple.bas"
50 IF RUN_TEST_1_OK != 1 || SHARED_VAR != 100 THEN LET ERROR = "Simple RUN failed" : END
60 PRINTLN "Simple RUN: OK"

70 REM 2. Test RUN LINE
80 LET RUN_TEST_2_OK = 0
90 RUN "run_test/sub_line.bas" LINE 40
100 IF RUN_TEST_2_OK != 1 || SHARED_VAR_2 != 200 THEN LET ERROR = "RUN LINE failed" : END
110 PRINTLN "RUN LINE: OK"

120 REM 3. Test Chained RUN (A -> B -> C)
130 LET CHAIN_VAR = 0
140 LET B_FINISHED = 0
150 LET C_FINISHED = 0
160 RUN "run_test/sub_b.bas"
170 IF CHAIN_VAR != 2 THEN LET ERROR = "Chain var count failed" : END
180 IF B_FINISHED != 1 || C_FINISHED != 1 THEN LET ERROR = "Chain execution order failed" : END
190 PRINTLN "Chained RUN (A->B->C): OK"

200 REM 4. Endless Loop exception test
210 PRINTLN "Testing Stack Overflow Exception..."
220 LET NEST_DEPTH = 0
230 RUN "run_test/sub_loop_a.bas"
240 REM The program should throw an exception before reaching here, so if it finishes cleanly, the exception logic failed!
250 IF NEST_DEPTH < 21 THEN LET ERROR = "Failed to hit recursion limit" : END
260 PRINTLN "Loop Test Reached Return!"

300 END