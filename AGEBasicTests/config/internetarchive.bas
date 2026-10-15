10 REM Internet Archive cabinet pack: list the cabinets and download the first one.
20 LET PACK = IALOAD("https://archive.org/details/cabinet-pack-1-1")
30 CLS
40 PRINT 0, 0, "Loading " + PACK + "..."
50 SLEEP 0.5
60 IF IASTATUS(PACK) = "LOADING" THEN GOTO 50
70 IF IASTATUS(PACK) = "NOTFOUND" || IASTATUS(PACK) = "FAILED" || IASTATUS(PACK) = "DARK" THEN GOTO 400

100 CLS
110 PRINTCENTERED 0, IAINFO(PACK, "title"), 1
120 PRINT 0, 1, "status: " + IASTATUS(PACK) + "  cabinets: " + STR(IAINFO(PACK, "count"))
130 LET CABS = IACABS(PACK)
140 IF LEN(CABS) = 0 THEN GOTO 400
150 FOR I = 0 TO LEN(CABS) - 1
160   PRINT 0, 3 + I, CABS[I] + " " + STR(IACABINFO(PACK, CABS[I], "size")) + " downloaded=" + STR(IACABINFO(PACK, CABS[I], "downloaded"))
170   PRINT 0, 3 + LEN(CABS) + I, "  " + IACABINFO(PACK, CABS[I], "description")
180 NEXT I
190 SHOW

200 REM download the first cabinet and follow the progress
210 LET FILE = IADOWNLOAD(PACK, CABS[0])
220 SLEEP 0.5
230 PRINT 0, 20, "download " + STR(INT(IADOWNLOADPROGRESS(PACK, FILE))) + "%   "
240 SHOW
250 IF IADOWNLOADSTATUS(PACK, FILE) = "QUEUED" || IADOWNLOADSTATUS(PACK, FILE) = "DOWNLOADING" THEN GOTO 220
260 PRINT 0, 21, "result: " + IADOWNLOADSTATUS(PACK, FILE) + "   "
270 PRINT 0, 22, "downloaded=" + STR(IACABINFO(PACK, CABS[0], "downloaded"))
280 PRINT 0, 24, "PRESS B to end", 1, 0
290 SHOW
300 IF ControlActive("JOYPAD_B") THEN END
310 SLEEP 0.1
320 GOTO 300

400 PRINT 0, 3, "pack not available: " + IASTATUS(PACK)
410 SHOW
420 END
