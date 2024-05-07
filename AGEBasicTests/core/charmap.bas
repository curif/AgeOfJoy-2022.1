10 CLS
15 PRINT screenWidth() - 1, ScreenHeight() - 1, "A"
20 LET charnum = 0
30 FOR row = 0 to ScreenHeight() - 1
40   PRINT 0, row, str(charnum)
50   FOR col = 4 to 16+4
60     LET strcharnum = str(charnum)
70     PRINT col, row, "\" + strcharnum
80     LET charnum = charnum + 1
90     IF charnum > 255 THEN GOTO 500
100   NEXT col
110   PRINT col + 2, row, str(charnum - 1)
120 NEXT row

500 PRINT col + 2, row, str(charnum - 1)
510 END

