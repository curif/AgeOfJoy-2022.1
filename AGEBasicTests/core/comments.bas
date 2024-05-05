10 rem coment
20 ' comment
30 let a = 10 'comment

40 if a = 1 then let msg = "error" ' es un error
   else if a = 10 then GOSUB 1000 ' es ok
   else let msg = "error 2" ' another error

50 END

1000 let msg = "ok"
1010 RETURN
