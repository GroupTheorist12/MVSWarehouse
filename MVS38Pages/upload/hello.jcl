//HERHE8 JOB (COBOL),
//             'Hello MVS 1',
//             CLASS=A,
//             MSGCLASS=A,
//             REGION=8M,TIME=1440,
//             MSGLEVEL=(1,1),
//  USER=HERC01,PASSWORD=CUL8TR
//HEMV1    EXEC COBUCG,
//         PARM.COB='FLAGW,LOAD,SUPMAP,SIZE=2048K,BUF=1024K'
//COB.SYSPUNCH DD DUMMY
//COB.SYSIN    DD *
000100 IDENTIFICATION DIVISION.
000200 PROGRAM-ID.     'HEMV1'.
000300 ENVIRONMENT DIVISION.
001000 DATA DIVISION.
100000 PROCEDURE DIVISION.
100100 00-MAIN.
100500     DISPLAY 'Hello MVS !'.
100600     STOP RUN.
 /*
//COB.SYSLIB   DD DSNAME=SYS1.COBLIB,DISP=SHR
//GO.SYSOUT DD SYSOUT=*
//GO.SYSIN  DD * 
    'Howdy'
/*
//
