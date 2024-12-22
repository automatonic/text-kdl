**document:**

![document](document.svg)

```
document ::= '\u{FEFF}'? nodes
```

**nodes:**

![nodes](nodes.svg)

```
nodes    ::= line-space* ( node line-space* )*
```

referenced by:

* document
* node-children

**node:**

![node](node.svg)

```
node     ::= base-node node-terminator
```

referenced by:

* nodes

**final-node:**

![final-node](final-node.svg)

```
final-node
         ::= base-node node-terminator?
```

referenced by:

* node-children

**base-node:**

![base-node](base-node.svg)

```
base-node
         ::= slashdash? type? node-space* string ( node-space+ slashdash? node-prop-or-arg )* ( node-space+ slashdash node-children )* ( node-space+ node-children )? ( node-space+ slashdash node-children )* node-space*
```

referenced by:

* final-node
* node

**node-prop-or-arg:**

![node-prop-or-arg](node-prop-or-arg.svg)

```
node-prop-or-arg
         ::= prop
           | value
```

referenced by:

* base-node

**node-children:**

![node-children](node-children.svg)

```
node-children
         ::= '{' nodes final-node? '}'
```

referenced by:

* base-node

**node-terminator:**

![node-terminator](node-terminator.svg)

```
node-terminator
         ::= single-line-comment
           | newline
           | ';'
           | eof
```

referenced by:

* final-node
* node

**prop:**

![prop](prop.svg)

```
prop     ::= string node-space* '=' node-space* value
```

referenced by:

* node-prop-or-arg

**value:**

![value](value.svg)

```
value    ::= type? node-space* ( string | number | keyword )
```

referenced by:

* node-prop-or-arg
* prop

**type:**

![type](type.svg)

```
type     ::= '(' node-space* string node-space* ')'
```

referenced by:

* base-node
* value

**string:**

![string](string.svg)

```
string   ::= identifier-string
           | quoted-string
           | raw-string
```

referenced by:

* base-node
* prop
* type
* value

**identifier-string:**

![identifier-string](identifier-string.svg)

```
identifier-string
         ::= unambiguous-ident
           | signed-ident
           | dotted-ident
```

referenced by:

* string

**quoted-string:**

![quoted-string](quoted-string.svg)

```
quoted-string
         ::= '"' single-line-string-body '"'
           | '"""' newline multi-line-string-body newline unicode-space* '"""'
```

referenced by:

* string

**single-line-string-body:**

![single-line-string-body](single-line-string-body.svg)

```
single-line-string-body
         ::= string-character-minus-newline*
```

referenced by:

* quoted-string

**multi-line-string-body:**

![multi-line-string-body](multi-line-string-body.svg)

```
multi-line-string-body
         ::= ( ( '"' | '""' )? string-character )*
```

referenced by:

* quoted-string

**string-character:**

![string-character](string-character.svg)

```
string-character
         ::= '\' escape
           | neither-quote-nor-disallowed-literal-code-points
```

referenced by:

* multi-line-string-body

**escape:**

![escape](escape.svg)

```
escape   ::= ["\bfnrts]
           | 'u{' hex-digit ( hex-digit ( hex-digit ( hex-digit ( hex-digit hex-digit? )? )? )? )? '}'
           | ( unicode-space | newline )+
```

referenced by:

* string-character

**hex-digit:**

![hex-digit](hex-digit.svg)

```
hex-digit
         ::= [0-9a-fA-F]
```

referenced by:

* escape
* hex

**raw-string:**

![raw-string](raw-string.svg)

```
raw-string
         ::= '#' ( raw-string-quotes | raw-string ) '#'
```

referenced by:

* raw-string
* string

**raw-string-quotes:**

![raw-string-quotes](raw-string-quotes.svg)

```
raw-string-quotes
         ::= '"' single-line-raw-string-body '"'
           | '"""' newline multi-line-raw-string-body '"""'
```

referenced by:

* raw-string

**single-line-raw-string-body:**

![single-line-raw-string-body](single-line-raw-string-body.svg)

```
single-line-raw-string-body
         ::= ''
           | '"'? single-line-raw-string-char-minus-double-quote single-line-raw-string-char*
```

referenced by:

* raw-string-quotes

**number:**

![number](number.svg)

```
number   ::= keyword-number
           | hex
           | octal
           | binary
           | decimal
```

referenced by:

* value

**decimal:**

![decimal](decimal.svg)

```
decimal  ::= sign? integer ( '.' integer )? exponent?
```

referenced by:

* number

**exponent:**

![exponent](exponent.svg)

```
exponent ::= ( 'e' | 'E' ) sign? integer
```

referenced by:

* decimal

**integer:**

![integer](integer.svg)

```
integer  ::= digit ( digit | '_' )*
```

referenced by:

* decimal
* exponent

**digit:**

![digit](digit.svg)

```
digit    ::= [0-9]
```

referenced by:

* integer

**sign:**

![sign](sign.svg)

```
sign     ::= '+'
           | '-'
```

referenced by:

* binary
* decimal
* exponent
* hex
* octal

**hex:**

![hex](hex.svg)

```
hex      ::= sign? '0x' hex-digit ( hex-digit | '_' )*
```

referenced by:

* number

**octal:**

![octal](octal.svg)

```
octal    ::= sign? '0o' [0-7] [0-7_]*
```

referenced by:

* number

**binary:**

![binary](binary.svg)

```
binary   ::= sign? '0b' ( '0' | '1' ) ( '0' | '1' | '_' )*
```

referenced by:

* number

**keyword:**

![keyword](keyword.svg)

```
keyword  ::= boolean
           | '#null'
```

referenced by:

* value

**keyword-number:**

![keyword-number](keyword-number.svg)

```
keyword-number
         ::= '#inf'
           | '#-inf'
           | '#nan'
```

referenced by:

* number

**boolean:**

![boolean](boolean.svg)

```
boolean  ::= '#true'
           | '#false'
```

referenced by:

* keyword

**single-line-comment:**

![single-line-comment](single-line-comment.svg)

```
single-line-comment
         ::= '//' anything-but-newline* ( newline | eof )
```

referenced by:

* escline
* line-space
* node-terminator

**multi-line-comment:**

![multi-line-comment](multi-line-comment.svg)

```
multi-line-comment
         ::= '/*' ( multi-line-comment | '*' | '/' | not-block-end+ )* '*/'
```

referenced by:

* multi-line-comment
* ws

**slashdash:**

![slashdash](slashdash.svg)

```
slashdash
         ::= '/-' ( node-space | line-space )*
```

referenced by:

* base-node

**ws:**

![ws](ws.svg)

```
ws       ::= unicode-space
           | multi-line-comment
```

referenced by:

* escline
* line-space
* node-space

**escline:**

![escline](escline.svg)

```
escline  ::= '\\' ws* ( single-line-comment | newline | eof )
```

referenced by:

* node-space

**line-space:**

![line-space](line-space.svg)

```
line-space
         ::= newline
           | ws
           | single-line-comment
```

referenced by:

* nodes
* slashdash

**node-space:**

![node-space](node-space.svg)

```
node-space
         ::= ws* ( escline ws* | ws )
```

referenced by:

* base-node
* prop
* slashdash
* type
* value

## 
![rr-2.1](rr-2.1.svg) <sup>generated by [RR - Railroad Diagram Generator][RR]</sup>

[RR]: https://www.bottlecaps.de/rr/ui