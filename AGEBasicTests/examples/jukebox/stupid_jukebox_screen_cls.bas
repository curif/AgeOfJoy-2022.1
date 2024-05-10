10 CLS

020 print 0,  1, "  ____  __ __  __  _    ___" , 0,0
030 print 0,  2, " |    ||  T  T|  |/ |  /  _|", 0,0
040 print 0,  3, " l__  ||  |  ||  ' /  /  |_ ", 0,0
050 print 0,  4, " __j  ||  |  ||    \\ Y    _|", 0,0
060 print 0,  5, "/  |  ||  :  ||     Y|   |_ ", 0,0
070 print 0,  6, "\\  `  |l     ||  .  ||     T", 0,0
080 print 0,  7, " \____j \__,_jl__j\_jl_____j", 0,0
090 print 0,  10, "     ____    ___   __ __"    , 0,0
100 print 0,  11, "    |    \\  /   \\ |  T  T"   , 0,0
110 print 0,  12, "    |  o  )Y     Y|  |  |"   , 0,0
120 print 0,  13, "    |     T|  O  |l_   _j"   , 0,0
130 print 0,  14, "    |  O  ||     ||     |"   , 0,0
140 print 0,  15, "    |     |l     !|  |  |"   , 0,0
150 print 0,  16, "    l_____j \___/ |__j__|", 0,0
160 SHOW

200 CLS
210 LET line = "\64" * ScreenWidth()
220 PRINT 0,10, line, 0, 0
230 PRINT 0,11, ("\64" * 15) + " JUKEBOX " + ("\64" * 16), 1, 0
240 PRINT 0,12, line, 0, 0
250 PRINT 9,14, "insert a coin to start", 0, 0
260 PRINT 0, ScreenHeight() - 1, "Playing " + str(MusicCount()) + " files.", 0, 0
270 SHOW