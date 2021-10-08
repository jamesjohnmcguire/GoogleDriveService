<?php
class CLI {
	# More Codes @ http://misc.flogisoft.com/bash/tip_colors_and_formatting
	#              http://www.termsys.demon.co.uk/vtansi.htm
	#              http://wiki.bash-hackers.org/scripting/terminalcodes

	static function black($s)   { return chr(27) . "[30m$s" . chr(27) . "[0m"; }
	static function red($s)     { return chr(27) . "[31m$s" . chr(27) . "[0m"; }
	static function green($s)   { return chr(27) . "[32m$s" . chr(27) . "[0m"; }
	static function yellow($s)  { return chr(27) . "[33m$s" . chr(27) . "[0m"; }
	static function blue($s)    { return chr(27) . "[34m$s" . chr(27) . "[0m"; }
	static function purple($s)  { return chr(27) . "[35m$s" . chr(27) . "[0m"; }
	static function cyan($s)    { return chr(27) . "[36m$s" . chr(27) . "[0m"; }
	static function gray($s)    { return chr(27) . "[37m$s" . chr(27) . "[0m"; }

	static function bblack($s)  { return chr(27) . "[90m$s" . chr(27) . "[0m"; }
	static function bred($s)    { return chr(27) . "[91m$s" . chr(27) . "[0m"; }
	static function bgreen($s)  { return chr(27) . "[92m$s" . chr(27) . "[0m"; }
	static function byellow($s) { return chr(27) . "[93m$s" . chr(27) . "[0m"; }
	static function bblue($s)   { return chr(27) . "[94m$s" . chr(27) . "[0m"; }
	static function bpurple($s) { return chr(27) . "[95m$s" . chr(27) . "[0m"; }
	static function bcyan($s)   { return chr(27) . "[96m$s" . chr(27) . "[0m"; }
	static function white($s)   { return chr(27) . "[97m$s" . chr(27) . "[0m"; }

	static function bold($s)    { return chr(27) . "[1m$s" . chr(27) . "[21m"; }
	static function dim($s)     { return chr(27) . "[2m$s" . chr(27) . "[22m"; }
	static function under($s)   { return chr(27) . "[4m$s" . chr(27) . "[24m"; }
	static function invert($s)  { return chr(27) . "[7m$s" . chr(27) . "[27m"; }

	static function startline() { echo chr(27) . "[1000D"; }  # Move 1000x to the left.
	static function clearline() { echo chr(27) . "[2K"; CLI::startline(); }

	static function pprint($o)  { ob_start(); print_r($o); $ob = ob_get_contents(); ob_end_clean(); return $ob; }

	static function lpad($s, $n = 8) { return str_pad($s, $n, " ", STR_PAD_LEFT); }
	static function rpad($s, $n = 8) { return str_pad($s, $n, " ", STR_PAD_RIGHT); }
}
