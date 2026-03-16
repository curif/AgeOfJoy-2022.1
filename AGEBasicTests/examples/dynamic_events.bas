10 REM Example program demonstrating the improved ONEVENT syntax
20 REM Using specialized functions for event registration
25 CLS
30 PRINTLN "Initializing Dynamic Event Handlers..."

40 REM Register a timer event that triggers every 5 seconds
45 LET ontimercount = 0
50 ONEVENT ONTIMER(5) GOTO 205
55 PRINTLN "ONTIMER event registered"

60 REM Register a control event: (ControlID, Type, Port)
70 REM Types: "pressed", "held", "released"
80 ONEVENT ONCONTROL("JOYPAD_Y", "pressed", 0) GOTO 300
85 PRINTLN "JOYPAD_Y event registered"

90 REM Register VR hand interaction events (needs some test. Does not work on configuration cabinet)
100 REM ONEVENT ONTOUCH("Button1") GOTO 400
110 REM ONEVENT ONGRAB("Joystick") GOTO 500

120 REM Main program ends here, events stay active
130 END

200 REM Timer Event Logic
205 CLS
210 LET ontimercount = ontimercount + 1
220 PRINTLN "Timer Event: 5 seconds passed! count:" + str(ontimercount)
230 IF ontimercount > 5 THEN STOP
240 END

300 REM Fire Button Event Logic
305 CLS
310 PRINTLN "Input Event: Fire button pressed!"
320 END

400 REM Touch Event Logic
410 PRINTLN "Touch Event: You touched Button1!"
420 END

500 REM Grab Event Logic
510 PRINTLN "Grab Event: You grabbed the Joystick!"
520 END
