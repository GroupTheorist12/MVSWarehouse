#![allow(dead_code)]

use std::fs;
use std::io::prelude::*;
use std::fs::File;
use std::process::Command;
use std::process;

const TKCONF:&str  = r#"
#**********************************************************************
#***                                                                ***
#*** File:    tk4-.cnf                                              ***
#***                                                                ***
#*** Purpose: Hercules configuration file for MVS 3.8j TK4-         ***
#***                                                                ***
#*** Updated: 2014/12/22                                            ***
#***                                                                ***
#**********************************************************************
CPUSERIAL 000611
CPUMODEL 3033
MAINSIZE ${MAINSIZE:=16}
XPNDSIZE 0
CNSLPORT ${CNSLPORT:=3270}
HTTP PORT ${HTTPPORT:=8038}
HTTP ROOT hercules/httproot
HTTP START
NUMCPU ${NUMCPU:=1}
MAXCPU ${MAXCPU:=1}
TZOFFSET +0000
ARCHMODE S/370
OSTAILOR QUIET
DIAG8CMD ENABLE ECHO
# .-----------------------------Device number
# |    .------------------------Device type
# |    |   .--------------------File name
# |    |   |
# V    V   V
#--- ---- --------------------
#
# unit record devices
#
0002 3211 prt/prt002.txt ${TK4CRLF}
000E 1403 "|printtools/prtspool A hercules/httproot/jobs/"

000C 3505 ${RDRPORT:=3505} sockdev ascii trunc eof
000D 3525 pch/pch00d.txt ascii
0480 3420 *
010C 3505 jcl/dummy eof ascii trunc
010D 3525 pch/pch10d.txt ascii
000F 1403 prt/prt00f.txt ${TK4CRLF}
030E 1403 log/hardcopy.log ${TK4CRLF}
#
# consoles
#
INCLUDE conf/${TK4CONS:=intcons}.cnf
#
# local 3270 devices (VTAM)
#
00C0 3270
00C1 3270
00C2 3270
00C3 3270
00C4 3270
00C5 3270
00C6 3270
00C7 3287
#
# local 3270 terminals (TCAM)
#
03C0 3270 TCAM
03C1 3270 TCAM
03C2 3270 TCAM
03C3 3270 TCAM
03C4 3270 TCAM
03C5 3270 TCAM
03C6 3270 TCAM
03C7 3270 TCAM
#
# optional devices
#
INCLUDE ${CNF101A:=conf}/tk4-_${REP101A:=default}${CMD101A}.cnf
#
# TK4- DASD
#
0152 3330 dasd/hasp00.152
0191 3390 dasd/mvscat.191
0248 3350 dasd/mvsdlb.248
0148 3350 dasd/mvsres.148
0160 3340 dasd/page00.160
0161 3340 dasd/page01.161
0240 3350 dasd/pub000.240
0241 3350 dasd/pub010.241
0270 3375 dasd/pub001.270
0271 3375 dasd/pub011.271
0280 3380 dasd/pub002.280
0281 3380 dasd/pub012.281
0290 3390 dasd/pub003.290
0291 3390 dasd/pub013.291
0149 3350 dasd/smp001.149
014a 3350 dasd/smp002.14a
014b 3350 dasd/smp003.14b
014c 3350 dasd/smp004.14c
0131 2314 dasd/sort01.131
0132 2314 dasd/sort02.132
0133 2314 dasd/sort03.133
0134 2314 dasd/sort04.134
0135 2314 dasd/sort05.135
0136 2314 dasd/sort06.136
0140 3350 dasd/work00.140
0170 3375 dasd/work01.170
0180 3380 dasd/work02.180
0190 3390 dasd/work03.190
#
# CBT DASD
#
INCLUDE conf/cbt_dasd.cnf
#
# Source DASD
#
INCLUDE conf/source_dasd.cnf
#
# TK4- updates
#
INCLUDE conf/tk4-_updates.cnf
#
# local updates
#
${LOCALCNF:=INCLUDE conf/local.cnf}
"#;


const PRTQEUE:&str  = r#"
#include <stdio.h>
#include <string.h>

/* prtspool by Tim Pinkawa (http://timpinkawa.net/hercules)
 * Written in December 2006, released in June 2007
 */

int main(int argc, char* argv[])
{
  if(argc != 3 && argc != 4)
  {
	  printf("prtspool - a simple print spooler for emulated 1403 printers\n");
	  printf("By Tim Pinkawa (http://www.timpinkawa.net/hercules)\n\n");
	  printf("usage: %s {msgclass} {output_directory} [command]\n", argv[0]);
	  return 0;
  }
  int job = 1;
  while(!feof(stdin))
  {
    int endCount = 0;
    char line[200];
    char ss[200];
    fgets(line, 200, stdin);
    if(strcmp(line, "\f") == 0)
      break;

	 char jobEnd[15];
	 snprintf(jobEnd, 15, "****%s   END", argv[1]);

	 char cmd[225];
	 if(argc == 4)
		 snprintf(cmd, 225, "%s %s %s", argv[3], argv[1], argv[2]);
	 
    int i;
    for(i = 1; i < 200; i++)
      ss[i - 1] = line[i];
    FILE* jobfp;

    char path[250];
    snprintf(path, 250, "%s%s%i%s", argv[2], "job-", job, ".txt");

    jobfp = fopen(path, "w");
    fprintf(jobfp, ss);
    int endOfJob = 0;
    while(!endOfJob && !feof(stdin))
    {
      fgets(line, 200, stdin);
      if(strstr(line, jobEnd) != NULL)
        endCount++;
      if(endCount == 4)
        endOfJob = 1;
      fprintf(jobfp, line);
    }
    fclose(jobfp);

    if(argc == 4)
       system(cmd);

    job++;
  }
}
"#;

fn main() {

    let path = "printtools";
    if std::path::Path::new(&path).exists() {
        
        println!("printtools already exists. Setup was already ran");
        process::exit(1);
    }

    let path = "conf";
    if !std::path::Path::new(&path).exists() {
        
        println!("Run printer setup from mvs root directory");
        process::exit(2);
    }

    println!("Updating tk4-.cnf file");
    fs::rename("conf/tk4-.cnf", "conf/tk4-.cnf.bak").expect("file should exist");

    let mut buffer = File::create("conf/tk4-.cnf").expect("file to be written");

    buffer.write_all(TKCONF.as_bytes()).expect("bytes to be written");

    println!("Creating printtools directory");

    fs::create_dir("printtools").expect("printtool dir not created");

    println!("Creating prtspools binary");

    let mut buffer2 = File::create("printtools/prtspool.c").expect("file to be written");

    buffer2.write_all(PRTQEUE.as_bytes()).expect("bytes to be written");
    
    
    let output  = {
        Command::new("gcc")
                .arg("printtools/prtspool.c")
                .arg("-oprinttools/prtspool")
                .output()
                .expect("failed to execute process")
    };
    
    let _ = output.stdout;
    
    //fs::copy("prtspool", "prtspool2").expect("failed to copy file");
    println!("Print spool setup complete.");

}
