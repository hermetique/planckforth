h@l@h@!h@C+h!k1k0-h@$k
h@k1k0-+$h@C+h!ih@!h@C+h!kefh@!h@C+h!l!
h@l@h@!h@C+h!k1k0-h@$k h@k1k0-+$h@C+h!ih@!h@C+h!kefh@!h@C+h!l!

h@l@ h@!h@C+h! k1k0-h@$ k\h@k1k0-+$ h@C+h!
    i       h@!h@C+h!
    kkf     h@!h@C+h!
    kLf     h@!h@C+h!
    k:k0-   h@!h@C+h!
    k=f     h@!h@C+h!
    kJf     h@!h@C+h!
    k0k5-C* h@!h@C+h!
    kef     h@!h@C+h!
l!

\ **Now we can use single-line comments!**

\ planckforth -
\ Copyright (C) 2021 nineties

\ This project aims to bootstrap a Forth interpreter
\ from hand-written tiny ELF binary.

\ In the 1st stage, only single character words are registered
\ in the dictionary.
\ List of builtin words:
\ 'Q' ( -- )            Exit the process
\ 'C' ( -- n )          The size of Cells
\ 'h' ( -- a-addr )     The address of 'here' cell
\ 'l' ( -- a-addr )     The address of 'latest' cell
\ 'k' ( -- c )          Read character
\ 't' ( c -- )          Print character
\ 'j' ( -- )            Unconditional branch
\ 'J' ( n -- )          Jump if a == 0
\ 'f' ( c -- xt )       Get execution token of c
\ 'x' ( xt -- ... )     Run the execution token
\ '@' ( a-addr -- w )   Load value from addr
\ '!' ( w a-addr -- )   Store value to addr
\ '?' ( c-addr -- c )   Load byte from addr
\ '$' ( c c-addr -- )   Store byte to addr
\ 'd' ( -- a-addr )     Get data stack pointer
\ 'D' ( a-addr -- )     Set data stack pointer
\ 'r' ( -- a-addr )     Get return stack pointer
\ 'R' ( a-addr -- )     Set return stack pointer
\ 'i' ( -- a-addr )     Get the interpreter function
\ 'e' ( -- )            Exit current function
\ 'L' ( -- u )          Load immediate
\ 'S' ( -- a-addr u )   Load string literal
\ '+' ( a b -- c )      c = (a + b)
\ '-' ( a b -- c )      c = (a - b)
\ '*' ( a b -- c )      c = (a * b)
\ '/' ( a b -- c )      c = (a / b)
\ '%' ( a b -- c )      c = (a % b)
\ '&' ( a b -- c )      c = (a & b)
\ '|' ( a b -- c )      c = (a | b)
\ '^' ( a b -- c )      c = (a ^ b)
\ '<' ( a b -- c )      c = (a < b)
\ '=' ( a b -- c )      c = (a == b)

\ The 1st stage interpreter repeats execution of k, f and x.
\ There following line is an example program of planckforth
\ which prints "Hello World!\n"
\ --
\ kHtketkltkltkotk tkWtkotkrtkltkdtk!tk:k0-tQ
\ --
\ This code repeats that 'k' reads a character and 't' prints it.
\ Note that ':' (58) minus '0' (48) is '\n' (10).

\ The structure of the dictionary.
\ +------+----------+---------+------------+---------------+
\ | link | len+flag | name... | padding... | code field ...|
\ +------+----------+---------+------------+---------------+
\ - link pointer to the previous entry (CELL byte)
\ - length of the name (6 bits)
\ - smudge bit (1 bit)
\ - immediate bit (1 bit)
\ - characters of the name (N bits)
\ - padding to align CELL boundary if necessary.
\ - codewords and datawords (CELL-bye aligned)

\ The code group at the beginning of this file
\ defines ' ' and '\n' as no-op operation and
\ '\' to read following characters until '\n'.
\ Since I couldn't write a comment at the beginning,
\ I repost the definition of '\' for explanation.
\ --
\ h@                            ( save addr of new entry )
\ l@ h@!h@C+h!                  ( set link pointer. *here++ = latest )
\ k1k0-h@$ k\h@k1k0-+$ h@C+h!   ( write the name '\' and its length )
\ i       h@!h@C+h!             ( docol )
\ kkf     h@!h@C+h!             ( key )
\ kLf     h@!h@C+h!             ( lit )
\ k:k0-   h@!h@C+h!             ( '\n' )
\ k=f     h@!h@C+h!             ( = )
\ kJf     h@!h@C+h!             ( branch )
\ k0k5-C* h@!h@C+h!             ( -5*CELL )
\ kef     h@!h@C+h!             ( exit )
\ l!                            ( set latest to this new entry. )
\ --

\ That's all for the brief explanation. Let's restart bootstrap!

\ The COMMA operator
\ ',' ( a -- )  Store a to 'here' and increment 'here' CELL bytes.
h@l@ h@!h@C+h! k1k0-h@$ k,h@k1k0-+$ h@C+h!
    i   h@!h@C+h!   \ docol
    \ store 'a' to here
    khf h@!h@C+h!
    k@f h@!h@C+h!
    k!f h@!h@C+h!
    \ here <- here + CELL
    khf h@!h@C+h!
    k@f h@!h@C+h!
    kCf h@!h@C+h!
    k+f h@!h@C+h!
    khf h@!h@C+h!
    k!f h@!h@C+h!
    \ exit
    kef h@!h@C+h!
l!

\ TICK-like operator
\ '\'' ( "c" -- xt )    Get execution token of following character
\ NB: This definition is different from the usual definition of tick
\ because it does not skip leading spaces and can read only a single
\ character. It will be redefined in later stage.
h@l@, k1k0-h@$ k'h@k1k0-+$ h@C+h!
    i, kkf, kff, kef,
l!

\ Utility for defining a word
\ 'c' ( "c" -- w )
\ Read character, create new word then push its address.
\ 'latest' will not be updated.
h@l@, k1k0-h@$ kch@k1k0-+$ h@C+h!
    i, 'h, '@, 'l, '@, ',,
    'L, k1k0-, 'h, '@, '$,
    'k, 'h, '@, 'L, k1k0-, '+, '$,
    'h, '@, 'C, '+, 'h, '!,
'e, l!

\ '_' ( a -- ) DROP
c_ i, 'd, 'C, '+, 'D, 'e, l!

\ '#' ( a -- a a )  DUP
c# i, 'd, '@, 'e, l!



\ Implementations of TOR and FROMR are a bit tricky.
\ Since return-address will be placed at the top of return stack,
\ the code in the body of these function have to manipulate
\ 2nd element of the stack.

\ '{' ( a -- R:a ) TOR
\ Move value from data stack to return stack.
c{ i,
    'r, 'r, '@,     \ ( a rsp ret )
    'r, 'C, '-, '#, \ ( a rsp ret rsp-1 rsp-1 )
    'R,             \ ( a rsp+1 ret rsp ) extend return stack
    '!,             \ ( a rsp+1 ) store return address to the top
    '!,             \ store a to the 2nd
'e, l!

\ '}' ( R:a -- a ) FROMR
\ Move value from return stack to data stack.
c} i,
    'r, 'C, '+, '@, \ ( a ) load 2nd value
    'r, '@,         \ ( a ret ) load return addr
    'r, 'C, '+, '#, \ ( a ret rsp+1 rsp+1 )
    'R,             \ ( a ret rsp ) reduce return stack
    '!,             \ ( a , R:ret ) store return addr to top of return stack
'e, l!

\ 'o' ( a b -- a b a ) OVER
co i, 'd, 'C, '+, '@, 'e, l!

\ '~' ( a b -- b a ) SWAP
c~ i,
    'o,             \ ( a b a )
    '{,             \ ( a b , R:a )
    'd, 'C, '+,     \ ( a b sp+1 , R:a )
    '!,             \ ( b , R:a )
    '},             \ ( b a )
'e, l!

\ 'B' ( c -- ) C-COMMA
\ Store byte 'c' to here and increment it
cB i, 'h, '@, '$, 'h, '@, 'L, k1k0-, '+, 'h, '!, 'e, l!

\ 'm' ( c-addr u -- ) CMOVE,
\ Copy u bytes from c-addr to here,
\ increment here u bytes.
cm i,
\ <loop>
    '#, 'J, k>k0-C*,        \ goto <exit> if u=0
        '{,                 \ preserve u
        '#, '?, 'B,         \ copy byte
        'L, k1k0-, '+,      \ increment c-addr
        '}, 'L, k1k0-, '-,  \ decrement u
        'j, k0k?-C*,        \ goto <loop>
\ <exit>
    '_, '_,
'e, l!

\ 'a' ( c-addr -- a-addr ) ALIGNED
\ Round up to a nearlest multiple of CELL
ca i,
    'L, Ck1k0--, '+,    \ ( a+CELL-1 )
    'L, k0k0-C-,        \ ( a+CELL-1 ~(CELL-1) )
    '&,
'e, l!

\ 'A' ( -- ) ALIGN
\ Round up 'here' to a nearlest multiple of CELL
cA i, 'h, '@, 'a, 'h, '!, 'e, l!

\ 'E' ( c-addr1 u1 c-addr2 u2 -- n ) STR=
\ Compare two strings.
\ Return 1 if they are same 0 otherwise.
cE i,
    '{, '~, '},                 \ ( c-addr1 c-addr2 u1 u2 )
    'o, '=, 'J, kVk0-C*,        \ jump to <not_equal> if u1!=u2
\ <loop>
        \ ( c-addr1 c-addr2 u )
        '#, 'J, kMk0-C*,        \ jump to <equal> if u==0
            '{,                 \ preserve u
            'o, '?,             \ ( c-addr1 c-addr2 c1 )
            'o, '?,             \ ( c-addr1 c-addr2 c1 c2 )
            '},                 \ ( c-addr1 c-addr2 c1 c2 u ) restore u
            '~, '{, '~, '},     \ ( c-addr1 c-addr2 u c1 c2 )
            '=, 'J, kFk0-C*,    \ jump to <not_equal> if c1!=c2
            '{, '{,             \ ( c-addr1 , R:u c-addr2 )
            'L, k1k0-, '+,      \ increment c-addr1
            '}, 'L, k1k0-, '+,  \ increment c-addrr2
            '}, 'L, k1k0-, '-,  \ decrement u
            'j, k0kN-C*,        \ jump to <loop>
\ <equal>
    '_, '_, '_, 'L, k1k0-, 'e,
\ <not_equal>
    '_, '_, '_, 'L, k0k0-, 'e,
l!

\ 's' ( c -- n)
\ Return 1 if c==' ' or c=='\n', 0 otherwise.
cs i, '#, 'L, k , '=, '~, 'L, k:k0-, '=, '|, 'e, l!

\ 'W' ( "name" -- c-addr u )
\ Skip leading spaces (' ' and '\n'),
\ Read name, then return its address and length.
\ The maximum length of the name is 63. The behavior is undefined
\ when the name exceeds 63 characters,
\ Note that it returns the address of statically allocated buffer,
\ so the content will be overwritten each time 'w' executed.

\ Allocate buffer of 63 bytes or more,
\ push the address for compilation of 'w'
h@ # kok0-+ h! A
cW~
i,
    \ skip leading spaces
    'k, '#, 's, 'J, k4k0-C*, '_, 'j, k0k7-C*,
    \ p=address of buffer
    'L, #, '~,
\ <loop>
    \ ( p c )
    'o, '$,                     \ store c to p
    'L, k1k0-, '+,              \ increment p
    'k, '#, 's, 'J, k0k9-C*,    \ goto <loop> if c is not space
    '_, 'L, ,                   \ ( p buf )
    '~, 'o, '-,                 \ ( buf p-buf )
'e, l!

\ 'F' ( c-addr u -- w )
\ Lookup multi-character word from dictionary.
\ Return 0 if the word is not found.
\ Entries with smudge-bit=1 are ignored.
cF i,
    'l, '@,
\ <loop> ( addr u it )
    '#, 'J, kUk0-C*,        \ goto <exit> if it=NULL
        '#, 'C, '+, '?,     \ ( addr u it len+flag )
        'L, k@, '&,         \ test smudge-bit of it
        'J, k4k0-C*,
\ <1>
            \ smudge-bit=1
            '@,             \ load link
            'j, k0k>-C*,    \ goto <loop>
\ <2>
            \ smudge-bit=0
            '{, 'o, 'o, 'r, '@, '~, '{, '~, '}, '},
            \ ( addr u it addr u it )
            '#, 'L, Ck1k0-+, '+,        \ address of name
            '~, 'C, '+, '?,             \ length+flag
            'L, kok0-, '&,              \ take length (lower 6-bits)
            \ ( addr1 u1 it addr1 u1 addr2 u2 )
            'E, 'J, k0kJ-C*,            \ goto <1> if different name
\ <exit>
    '{, '_, '_, '}, \ Drop addr u return it
'e, l!

\ 'G' ( w -- xt )
\ Get CFA of the word
cG i,
    'C, '+, '#, '?, \ ( addr len+flag )
    'L, kok0-, '&,  \ take length
    '+,             \ add length to the addr
    'L, k1k0-, '+,  \ add 1 to the addr (1byte for len+field)
    'a,             \ align
'e, l!

\ 'M' ( -- c-addr)
\ The state variable
\ 0: immediate mode
\ 1: compile mode
h@ k0k0-,   \ allocate 1 cell and fill 0
cM~ i, 'L, , 'e, l!

\ 'I'
\ The 2nd Stage Interpreter
cI i,
\ <loop>
    'W,                 \ read name from input
    'F,                 \ find word
    'M, '@,             \ read state
    'J, kAk0-C*,        \ goto <immediate> if state=0
\ <compile>
        '#, 'C, '+, '?, \ ( w len+flag )
        'L, k@k@+, '&,  \ test immediate bit
        'L, k0k0-, '=,
        'J, k5k0-C*,    \ goto <immediate> if immediate-bit=1
        'G, ',,         \ compile
        'j, k0kE-C*,    \ goto <loop>
\ <immediate>
        'G, 'x,         \ execute
        'j, k0kI-C*,    \ goto <loop>
l!

I \ Enter 2nd Stage

\ === 2nd Stage Interpreter ===

r C + R     \ Drop 1st stage interpreter from call stack

\ '\'' ( "name" -- xt )
\ Redefine existing '\'' which uses 'k' and 'f'
\ to use 'W' and 'F'.
c ' i , ' W , ' F , ' G , ' e , l !

\ [ immediate ( -- )
\ Switch to immediate mode
c [ i , ' L , k 0 k 0 - , ' M , ' ! , ' e , l !
\ Set immediate-bit of [
l @ C + # { ? k @ k @ + | } $

\ ] ( -- )
\ Switch to compile mode
c ] i , ' L , k 1 k 0 - , ' M , ' ! , ' e , l !

\ : ( "name" -- ) COLON
\ Read name, create word with smudge=1,
\ compile 'docol' and enter compile mode.
c : i ,
    ' A ,               \ align here
    ' h , ' @ ,
    ' l , ' @ , ' , ,   \ fill link
    ' l , ' ! ,         \ update latest
    ' W ,               \ read name ( addr len )
    ' # ,               \ ( addr len len )
    ' L , k @ , ' | ,
    ' B ,               \ fill length + smudge-bit
    ' m ,               \ fill name
    ' A ,               \ align here
    ' i , ' , ,         \ compile docol
    ' ] ,               \ enter compile mode
' e , l !

\ ; ( -- ) SEMICOLON
\ Compile 'exit', unsmudge latest, and enter immediate mode.
c ; i ,
    ' A ,               \ align here
    ' L , ' e , ' , ,   \ compile exit
    ' l , ' @ ,
    ' C , ' + , ' # , ' ? ,
    ' L , k [ k d + ,   \ 0xbf
    ' & , ' ~ , ' $ ,   \ unsmudge
    ' [ ,               \ enter immediate mode
' e , l !
\ Set immediate-bit of ';'
l @ C + # { ? k @ k @ + | } $

: immediate-bit [ ' L , k @ k @ + , ] ; \ 0x80
: smudge-bit    [ ' L , k @ , ] ;       \ 0x40
: length-mask   [ ' L , k o k 0 - , ] ; \ 0x3f

\ ( "name" -- )
: set-immediate
    W F C + # { ? immediate-bit | } $
;

\ Set immediate-bit of single-line comment word \
\ so that we can write comments in compile-mode.
set-immediate \

\ Set immediate-bit of 'latest'
: immediate
    l @ C + # { ? immediate-bit | } $
;

: alias-builtin \ ( "name-new" "name-old" -- )
    \ Create new word "name-new".
    \ Copy code pointer of builtin word "name-old" to
    \ the new word "name-new".
    \ "name-old" must not be a FORTH word.
    A h @ l @ , l ! \ fill link, update latest
    W # B m A       \ fill length and chars of "name-new"
    W F G @ ,       \ fill code-pointer of "name-old"
;

\ Add new names to builtin primities.
\ Instead of defining as a new FORTH word like shown below,
\ the aliases ared created by copying their code-pointer.
\ : new-name old-name ;
\ Primitive operators which manipulate program counter and return stack
\ can not be defined as a FORTH word.

alias-builtin bye       Q
alias-builtin cell      C
alias-builtin &here     h
alias-builtin &latest   l
alias-builtin key       k
alias-builtin emit      t
alias-builtin branch    j
alias-builtin 0branch   J
alias-builtin execute   x
alias-builtin c@        ?
alias-builtin c!        $
alias-builtin sp@       d
alias-builtin sp!       D
alias-builtin rp@       r
alias-builtin rp!       R
alias-builtin docol     i
alias-builtin exit      e
alias-builtin lit       L
alias-builtin string    S
alias-builtin div       /
alias-builtin mod       %
alias-builtin and       &
alias-builtin or        |
alias-builtin xor       ^

\ Rename existing FORTH words
: word      W ;
: find      F ;
: >cfa      G ;
: c,        B ;
: cmove,    m ;
: state     M ;

: here      &here @ ;
: latest    &latest @ ;

\ === Compilers ===

\ compile: ( n -- )
\ runtime: ( -- n )
: literal
    lit lit ,   \ compile lit
    ,           \ compile n
; immediate

\ compile: ( "name" -- )
\ '[compile] word' compiles word *now* even if it is immediate
: [compile]
    ' ,
; immediate

\ ( xt -- )
\ postpone compilation of xt
: compile-helper
    [compile] literal   \ compile 'literal'
    [ ' , ] literal ,   \ compile ,
;

\ compile: ( "name" -- )
\ 'compile word' compiles word *later* even if it is immediate
: compile
    ' compile-helper
; immediate

\ ( -- xt )
: :noname
    here
    [ docol ] literal , \ compile docol
    ]                   \ enter compile mode
;

\ ( "name" -- xt )
\ compile time tick
: [']
    '                   \ read name and get xt
    [compile] literal   \ call literal
; immediate

\ === Constants ===

\ Since we don't have integer literals yet,
\ define small integer words for convenience
\ and readability.
: 0 [ key 0 key 0 - ] literal ;
: 1 [ key 1 key 0 - ] literal ;
: 2 [ key 2 key 0 - ] literal ;
: 3 [ key 3 key 0 - ] literal ;

: true 1 ;
: false 0 ;

\ === Address Arithmetic ===

: cell+ cell + ;
: cell- cell - ;
: cells cell * ;

\ ( c-addr -- a-addr )
\ Round up to nearlest multiple of CELL
: aligned
    cell + 1 -
    0 cell -
    and
;

\ ( -- )
\ Round up 'here' to nearlest multiple to CELL
: align here aligned &here !  ;

\ === Stack Manipulation ===

: drop  sp@ cell+ sp! ;     \ ( w -- )
: dup   sp@ @ ;             \ ( w -- w w )

: >r rp@ rp@ @ rp@ cell - dup rp! ! ! ;         \ ( w -- R:w )
: r> rp@ cell + @ rp@ @ rp@  cell + dup rp! ! ; \ ( R:w -- w)

: swap  sp@ cell + dup @ >r ! r> ;  \ ( a b -- b a )
: rot   >r swap r> swap ;           \ ( a b c -- b c a )
: -rot  swap >r swap r> ;           \ ( a b c -- c a b )
: nip   swap drop ;                 \ ( a b -- a )
: over  >r dup r> swap ;            \ ( a b -- a b a )
: tuck  dup -rot ;                  \ ( a b -- b a b )
: pick  cells sp@ swap + cell + @ ; \ ( wu ... x0 u -- xu ... x0 xu )

: 2drop drop drop ;                 \ ( a b -- )
: 2dup  over over ;                 \ ( a b -- a b a b )
: 2swap >r -rot r> -rot ;           \ ( a b c d -- c d a b )
: 2nip  2swap 2drop ;               \ ( a b c d -- c d )
: 2over 3 pick 3 pick ;             \ ( a b c d -- a b c d a b )
: 2tuck 2swap 2over ;               \ ( a b c d -- c d a b c d )
: 2rot  >r >r 2swap r> r> 2swap ;   \ ( a b c d e f -- c d e f a b )
: -2rot 2swap >r >r 2swap r> r> ;   \ ( a b c d e f -- e f a b c d )

: rdrop r> rp@ ! ;  \ ( R:w -- )

\ ( -- a-addr )
\ The bottom address of stacks.
\ sp@ and rp@ points bottom if implementation so far is correct.
: sp0 [ sp@ ] literal ;
: rp0 [ rp@ ] literal ;

\ === Integer Arithmetic ===

: 1+ 1 + ;
: 1- 1 - ;

\ ( a b -- (a mod b) (a / b) )
: /mod 2dup mod -rot / ;

\ ( n -- -n )
: negate 0 swap - ;

\ ( n1 -- n2 )
: not false = ;

: >     swap < ;
: <=    > not ;
: >=    < not ;
: <>    = not ;

: 0=    0 = ;
: 0<>   0 <> ;
: 0<    0 < ;
: 0>    0 > ;
: 0<=   0 <= ;
: 0>=   0 >= ;

\ ( a b c -- (a<=c & c<b) )
: within tuck > -rot <= and ;

\ === Conditional Branch ===
\ <condition> if <if-true> then
\ <condition> if <if-true> else <if-false> then
\ <condition> unless <if-false> then
\ <condition> unless <if-false> else <if-true> then

\ compile: ( -- orig )
\ runtime: ( n -- )
: if
    compile 0branch
    here 0 ,    \ save location of offset, fill dummy
; immediate

\ compile: ( orig -- )
\ runtime: ( -- )
: then
    here        \ ( orig dest )
    over -      \ ( orig offset )
    swap !      \ fill offset to orig
; immediate

\ compile: ( orig1 -- orig2 )
\ runtime: ( -- )
: else
    compile branch
    here 0 ,    \ save location of offset, fill dummy
    swap
    \ fill offset, here-orig1, to orig1
    here
    over -
    swap !
; immediate

\ compile: ( -- orig )
\ runtime: ( n -- )
: unless
    compile not
    [compile] if
; immediate

\ ( n -- n n | n )
\ duplicate if n<>0
: ?dup dup if dup then ;

\ === Loops ===
\ begin <body> <condition> until
\ begin <body> again
\ begin <condition> while <body> repeat

\ compile: ( -- dest )
\ runtime: ( -- )
: begin
    here        \ save location
; immediate

\ compile: ( dest -- )
\ runtime: ( n -- )
: until
    compile 0branch
    here - ,    \ fill offset
; immediate

\ compile: ( dest -- )
\ runtime: ( -- )
: again
    compile branch
    here - ,    \ fill offset
; immediate

\ compile: ( dest -- dest orig )
\ runtime: ( n -- )
\ dest=location of begin
\ orig=location of while
: while
    compile 0branch
    here 0 ,        \ save location, fill dummy
; immediate

\ compile: ( dest orig -- )
\ runtime: ( -- )
\ dest=location of begin
\ orig=location of while
: repeat
    swap
    compile branch
    here - ,                \ fill offset from here to begin
    here over - swap !      \ backfill offset from while to here
; immediate

\ === Recursive Call ===

\ recursive call.
\ compiles xt of current definition
: recurse
    latest >cfa ,
; immediate

\ === Case ===

\ ---
\ <value> case
\   <value1> of <case1> endof
\   <value2> of <case2> endof
\   ...
\   <default case>
\ endcase
\ ---
\ This is equivalent to
\ ---
\ <value>
\ <value1> over = if drop <case1> else
\ <value2> over = if drop <case2> else
\ ...
\ <default case>
\ then ... then then
\ ---


\ compile: ( -- 0 )
\ runtime: ( n -- )
: case
    0       \ push 0 to indicate there is no more case
; immediate

\ compile: ( -- orig )
: of
    compile over
    compile =
    [compile] if
    compile drop
; immediate

\ <value> a b rangeof <body> endof
\ Execute <body> when
\ a <= <value> and <value> <= b
: rangeof
    compile 2
    compile pick
    compile >=
    compile swap
    compile 2
    compile pick
    compile <=
    compile and
    [compile] if
    compile drop
; immediate

\ compile: ( orig1 -- orig2 )
: endof
    [compile] else
; immediate

: endcase
    compile drop
    begin ?dup while
        [compile] then
    repeat
; immediate

\ === Multiline Comment ===

: '('   [ key ( ] literal ;
: ')'   [ key ) ] literal ;

: (
    1   \ depth counter
    begin ?dup while
        key case
        '(' of 1+ endof \ increment depth
        ')' of 1- endof \ decrement depth
        endcase
    repeat
; immediate

(
    Now we can use multiline comment with ( nests. )
)

( === Memory Operation === )

: +! ( n a-addr -- ) tuck @ + swap ! ;
: -! ( n a-addr -- ) tuck @ - swap ! ;

\ allocate n bytes
: allot ( n -- c-addr )
    here swap
    &here +!
;

( === create and does> === )

\ no-operation
: nop ;

\ ( "name" -- )
\ Read name and create new dictionary entry.
\ When the word is executed, it pushs value of here
\ at the end of the entry.
: create
    align
    latest ,                \ fill link
    here cell- &latest !    \ update latest
    word
    dup c, cmove, align     \ fill length and name
    docol ,                 \ compile docol
    ['] lit ,
    here 3 cells + ,        \ compile the address
    ['] nop ,               \ does>, if any, will fill this cell
    ['] exit ,              \ compile exit
;

: does>-helper
    latest >cfa
    3 cells + tuck !    \ replace nop
;

: does>
    align
    0 [compile] literal \ literal for xt
    here cell-          \ save addr of xt

    \ fill xt and exit after docol of latest
    compile does>-helper

    [compile] ; \ finish compilation of initialization part
    :noname     \ start compilation of does> part
    swap !      \ backfill xt to the operand of literal
; immediate

( === Variable and Constant === )

\ ( "name" -- )
: variable create 0 , ;

\ ( n "name" -- )
: constant create , does> @ ;

( === String === )

\ Skip reading spaces, read characters and returns first character
: char      ( <spces>ccc -- c ) word drop c@ ;

\ compile-time version of char
: [char]    ( compile: <spaces>ccc -- ; runtime: --- c )
    char
    [compile] literal
; immediate


: '\n' [ key : key 0 - ] literal ; \ neline (10)
: bl   [ key P key 0 - ] literal ; \ space (32)
: '"'  [char] "" ;

: cr    '\n' emit ;
: space bl emit ;

\ Print string
: type ( c-addr u -- )
    begin dup 0> while   \ while u>0
        over c@ emit    \ print char
        1-              \ decrement u
        swap 1+ swap    \ increment c-addr
    repeat
;

\ Parse string delimited by "
\ compile mode: the string is stored as operand of 'string' operator.
\ immediate mode: the string is stored to temporary buffer.
: s"
    state @ if
        compile string
        here 0 ,    \ save location of length and fill dummy
        0           \ length of the string
        begin key dup '"' <> while
            c,      \ store character
            1+      \ increment length
        repeat drop
        swap !      \ back-fill length
        align
    else
        here dup    \ save start address
        begin key dup '"' <> while
            over c! \ store char
            1+      \ increment address
        repeat drop
        \ ( start-addr last-addr )
        over -      \ calculate length
    then
; immediate

\ Print string delimited by "
: ."
    [compile] s"
    state @ if
        compile type
    else
        type
    then
; immediate

