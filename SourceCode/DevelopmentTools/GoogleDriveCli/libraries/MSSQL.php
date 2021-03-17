<?php

# A cheap port of the original MySQL.php class v.1.3
# All MySQL functions are not portable to this

class MSSQL {
	private $database;
	private $connection;
	private $username;
	private $password;
	private $hostname;
	private $new_link;
	private $debugging;
	private $prefix;
	private $function;

	/**
	 * Query constructor flag to join pieces with an AND keyword.
	 *
	 */
	const SELECTOR_AND = 1;

	/**
	 * Query constructor flag to join pieces with an OR keyword.
	 *
	 */
	const SELECTOR_OR = 2;

	/**
	 * Query constructor flag to join pieces without a logical operator, just a comma.
	 * (Used internally for update queries).
	 *
	 */
	const SELECTOR_COMMA = 4;

	/**
	 * Query constructor flag to compare values with =.
	 *
	 */
	const SELECTOR_EQUAL = 8;

	/**
	 * Query constructor flag to compare values with LIKE.
	 *
	 */
	const SELECTOR_LIKE = 16;

	/**
	 * Query constructor flag to compare values with LIKE '% ... %'.
	 *
	 */
	const SELECTOR_MATCH = 32;

	/**
	 * Query constructor flag to compare values with !=.
	 *
	 */
	const SELECTOR_NOTEQUAL = 64;

	/**
	 * MSSQL constructor.
	 *
	 * @param string $database - Name of the database to use.
	 * @param string $username - MSSQL user name. (Default: 'root').
	 * @param string $password - MSSQL password. (Default: '' - No password.).
	 * @param string $hostname - Hostname of the server to connect to. (Default: 'localhost').
	 * @param bool $new_link - Create a new connection link. (Default: false - reuse any existing ones.).
	 * @return MSSQL
	 */
	public function __construct($database, $username = 'root',
		$password = '', $hostname = 'localhost', $new_link = false)
	{
		$this->database = $database;
		$this->username = $username;
		$this->password = $password;
		$this->hostname = $hostname;
		$this->new_link = $new_link;
		$this->connect();
	}

	private function connect()
	{

		if (function_exists("sqlsrv_connect"))
		{
			$connectionInfo = array( "Database"=> $this->database,
				"UID"=> $this->username, "PWD"=> $this->password);
			$this->connection =
				sqlsrv_connect('101.0.102.42', $connectionInfo);

			if (true == $this->connection)
			{
				$this->function = "MSSQL";
			}
			else
			{
				$this->error("Could not connect to server.");
			}
		}
		elseif (class_exists("PDO"))
		{
			$this->function = "PDO";
			try
			{
				$this->function = "PDO";
				$connectionString = "sqlsrv:Server=host=$this->hostname;".
					"Database=$this->database;ConnectionPooling=0";
				$this->connection = new PDO($connectionString,
					$this->username, $this->password);
				$this->connection->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
			}
			catch (PDOException $e)
			{
				var_dump($e);
				$this->error("Could not connect to server.");
			}
			catch(Throwable $error)
			{
				// PHP 7.x
				var_dump($error);
				$this->error("Could not connect to server.");
			}
			catch(Exception $error)
			{
				// PHP 5.x
				var_dump($error);
				$this->error("Could not connect to server.");
			}
		} else {
			$this->error("PHP MSSQL not supported in this system.");
		}
	}

	/**
	 * MSSQL destructor.
	 *
	 */
	public function __destruct() {
		# No need to close it manually really...
		# mysql_close($this->connection);
	}

	/**
	 * Trigger an error caused by MSSQL.
	 *
	 * @param string $error_type - Error origin.
	 */
	private function error($error_type) {
		$msg = 'Unable to complete MSSQL operation: ' . $error_type;
		trigger_error($msg, E_USER_WARNING);
	}

	/**
	 * Set a prefix for all table names. Any subsequent constructed queries
	 * will carry this value prepended to the table name.
	 *
	 * @param string $prefix - Table prefix. (Default: null - Remove any prefix set.)
	 */
	public function setTablePrefix($prefix = null) {
		$this->prefix = $prefix;
	}

	/**
	 * Get the name of the table, applying a prefix if available. Will not be
	 * applied if the given name already includes the same prefix.
	 *
	 * @param string $table - The original table name.
	 */
	private function getTableName($table) {
		if (strlen($this->prefix)) {
			if (substr($table, 0, strlen($this->prefix)) != $this->prefix) {
				return $this->prefix . $table;
			}
		}
		return $table;
	}

	/**
	 * Set the charset for subsequent queries to the server.
	 *
	 * @param string $charset - Character set name.
	 */
	public function setCharset($charset) {
		if ($this->function == "MSSQL") {
			mssql_set_charset($charset, $this->connection);
		} else {
			$this->error("Not implemented.\n");
		}
	}

	/**
	 * Enable/disable debugging output of the queries sent to the database.
	 *
	 * @param bool $enabled - Enable/Disable
	 */
	public function setDebugging($enabled = true) {
		$this->debugging = $enabled;
	}

	/**
	 * Execute a query and get the result back.
	 *
	 * @param string $query - Full query to execute.
	 * @return resource - Query result.
	 */
	public function executeQuery($query)
	{
		$result = null;

		if (!is_resource($this->connection) && !is_object($this->connection))
		{
			echo "no connection\r\n";
		}
		else
		{
			if ($this->debugging)
			{
				echo $query . "\n";
			}

			$query = trim($query);

			if ($this->function == "MSSQL")
			{
				$result = sqlsrv_query($this->connection, $query);

				if (false == $result)
				{
					$this->error("Query execution failed.");
				}
			}
			else
			{
				try
				{
					$result = $this->connection->prepare($query);
					$result->execute();
				}
				catch (PDOException $e)
				{
					$this->error("Query execution failed.");
				}
			}
		}

		return $result;
	}

	/**
	 * Simple wrapper for a common SELECT query.
	 *
	 * @param string $table - Table to select from.
	 * @param mixed $selector - Array with field and values to match, or selector string (Default: null - Matches everything.).
	 * @param int $limit - Selection limit. (Default: 0 - Unlimited).
	 * @param int $offset - Selection offset. (Default: 0).
	 * @param int $flags - Query construction flags. (Default: SELECTOR_AND | SELECTOR_EQUAL).
	 * @param mixed $order - Array with order field as array(field => "ASC"/"DESC"),
	 * or a string such as "RAND()". (Default: null - None.).
	 * @return resource - Result of the constructed query.
	 */
	public function selectionQuery($table, $selector = null, $limit = 0,
		$offset = 0, $flags = 0, $order = null)
	{
		$table = $this->getTableName($table);
		$query = "SELECT";
		if ($limit > 0)
		{
			if ($offset > 0) die("Offset not supported");
			$query.= " TOP (" . intval($limit) . ") ";
		}
		$query.= " * FROM " . $this->tickString($table);
		if (is_array($selector) && sizeof($selector) > 0) {
			$query.= " WHERE " . $this->buildSelector($selector, $flags);
		} else if (is_string($selector)) {
			$query.= " WHERE " . $selector;
		}
		if (is_array($order) && sizeof($order) > 0) {
			$query.= " ORDER BY " . $this->buildOrder($order);
		} else if (is_string($order)) {
			$query.= " ORDER BY " . $order;
		}

		var_dump($query);
		return $this->executeQuery($query);
	}

	/**
	 * Fetch a single (first) row from a result set returned by a selector query.
	 *
	 * @param string $table - Table to select from.
	 * @param mixed $selector - Array with field and values to match, or selector string (Default: null - Matches everything.).
	 * @param int $flags - Query construction flags. (Default: SELECTOR_AND | SELECTOR_EQUAL).
	 * @param mixed $order - Array with order field as array(field => "ASC"/"DESC"),
	 * or a string such as "RAND()". (Default: null - None.).
	 * @return array - Array containing the data of the row or null if nothing is returnd.
	 */
	public function fetchSelectorRow(
		$table, $selector = null, $flags = 0, $order = null)
	{
		$table = $this->getTableName($table);
		$h = $this->selectionQuery($table, $selector, 1, 0, $flags, $order);

		if ($this->function == "MSSQL")
		{
			$hasData = sqlsrv_has_rows($h);
			if (true == $hasData)
			{
				$result = sqlsrv_fetch_array($h, SQLSRV_FETCH_ASSOC);
				var_dump($result);
				return $result;
			}
			else
			{
				$errors = sqlsrv_errors();
				echo "errors: ";
				var_dump($errors);
				return null;
			}
		}
		else
		{
			return $h->fetch(PDO::FETCH_ASSOC);
		}
	}

	/**
	 * Fetch a single (first) row from a result set returned by a raw query.
	 *
	 * @param string $query - The query to execute.
	 * @return array - Array containing the data of the row or null if nothing is returnd.
	 */
	public function fetchQueryRow($query) {
		$h = $this->executeQuery($query);
		if ($this->function == "MSSQL") {
			if ($this->countRows($h)) {
				return mssql_fetch_array($h, MSSQL_ASSOC);
			} else {
				return null;
			}
		} else {
			return $h->fetch(PDO::FETCH_ASSOC);
		}
	}

	public function fetchRows($result) {
		if ($this->function == "MSSQL") {
			$rows = array();
			while ($row = mssql_fetch_array($result, MSSQL_ASSOC)) {
				$rows[] = $row;
			}
			return $rows;
		} else {
			return $result->fetchAll(PDO::FETCH_ASSOC);
		}
	}

	/**
	 * Get the number of rows returned by a particular query.
	 *
	 * @param mixed $query - Query string or resource to count.
	 * @return int - Number of rows.
	 */
	public function countRows($query)
	{
		$count = null;

		if ($this->function != "MSSQL")
		{
			$this->error("Not implemented.");
		}
		if (is_string($query))
		{
			echo "trying string query\r\n";
			$count = sqlsrv_num_rows($this->executeQuery($query));
		}
		else if (is_resource($query) || is_object($query))
		{
			$count = sqlsrv_num_rows($query);
		}

		return $count;
	}

	/**
	 * Escape a query safe string according to the rules set
	 * by the MSSQL connection.
	 *
	 * @param string $string - String to escape.
	 * @return string - Escaped string.
	 */
	public function escapeString($string) {
		if (function_exists('get_magic_quotes_gpc') && get_magic_quotes_gpc()) {
			$string = stripslashes($string);
		}
		$string = str_replace("'", "''", $string);
		return $string;
	}

	/**
	 * Surround a string with ticks, useful for table or field names.
	 * The string will be escaped as well.
	 *
	 * @param string $string - String to convert.
	 * @return string - Converted string.
	 */
	public function tickString($string) {
		return "[" . $this->escapeString($string) . "]";
	}

	/**
	 * Surround a string with quotes, useful for query data.
	 * The string will be escaped as well.
	 *
	 * @param string $string - String to convert.
	 * @return string - Converted string.
	 */
	public function quoteString($string) {
		if (is_int($string) || is_float($string)) {
			return $string;
		}
		return "'" . $this->escapeString($string) . "'";
	}

	/**
	 * Surround a string with quotes and wildcards ('% ... %'),
	 * useful for data that has to be full-text matched.
	 * The string will be escaped as well.
	 *
	 * @param string $string - String to convert.
	 * @return string - Converted string.
	 */
	public function quoteMatchString($string) {
		return "'%" . $this->escapeString($string) . "%'";
	}

	/**
	 * Build a query-safe string out of an array of fields and values.
	 * Example:
	 * array('A' => 1, 'B' => 2)
	 * Will become:
	 * `A` = '1' AND `B` = '2'
	 *
	 * @param array $selector - Field + value array.
	 * @param int $flags - Query construction flags. (Default: SELECTOR_AND | SELECTOR_EQUAL).
	 * @return string - Query fragment.
	 */
	public function buildSelector($selector, $flags = 0) {
		if (is_string($selector)) {
			return $selector;
		}

		if (!is_array($selector)) {
			return "";
		}

		if (($flags & MSSQL::SELECTOR_LIKE) || ($flags & MSSQL::SELECTOR_MATCH)) {
			$comparer = " LIKE ";
		} else if ($flags & MSSQL::SELECTOR_NOTEQUAL) {
			$comparer = " != ";
		} else { // Default SELECTOR_EQUAL
			$comparer = " = ";
		}

		foreach ($selector as $column => $value) {
			// If a column-less parameter was given (numerical index), then treat
			// it as a direct SQL WHERE fragment, similar to the is_string() bit above,
			// but in this case, it will still be connected by AND/OR's to other fields.
			if (is_int($column)) {
				$fields[] = $value;
				continue;
			}

			// If a dot is given in the field name, make it so it follows the
			// `database`.`table`.`field` format, ticking each seciton.
			$column = explode('.', $column);
			foreach ($column as $column_key => $column_part) {
				$column[$column_key] = $this->tickString($column_part);
			}
			$column = implode('.', $column);
			// If a single value is passed, arrayize it.
			if (is_array($value)) {
				$values = $value;
			} else {
				$values = array($value);
			}

			// Start the selector subpart with "(" if there is more than one value to match.
			if (sizeof($values) > 1) {
				$string = '(';
			} else {
				$string = '';
			}

			// Build the subpart.
			foreach ($values as $value) {
				if (strlen($string) > 1) {
					// If there are several values for this field, they should
					// always be OR'ed. That is, can't have (id = 1 AND id = 2), but
					// you can have (id = 1 OR id = 2), except for complex operations
					// of course, but then, you wouldn't use this method for that...
					$string.= ' OR ';
				}
				$string.= $column . $comparer . (is_null($value) ? 'NULL' : (($flags & MSSQL::SELECTOR_MATCH) ? $this->quoteMatchString($value) : $this->quoteString($value)));
			}

			// Finish the selector subpart with ")" if there is more than one value to match.
			if (sizeof($values) > 1) {
				$string.= ')';
			}

			$fields[] = $string;
		}

		if ($flags & MSSQL::SELECTOR_OR) {
			$connector = " OR ";
		} else if ($flags & MSSQL::SELECTOR_COMMA) {
			$connector = ", ";
		} else { // Default SELECTOR_AND
			$connector = " AND ";
		}

		return implode($connector, $fields);
	}

	/**
	 * Build the limit/offset subquery.
	 *
	 * @param int $limit - Limit.
	 * @param int $offset - Offset.
	 * @return string - Limit subquery.
	 */
	public function buildLimiter($limit = 0, $offset = 0) {
		$query = "";
		if ($limit > 0) {
			if ($offset > 0) {
				$query.= $offset . ",";
			}
			$query.= $limit;
		}
		return $query;
	}

	/**
	 * Builds the order subquery based on the array(field => "ASC"/"DESC") given.
	 * It can also be a string to use a MSSQL function such as "RAND()".
	 *
	 * @param mixed $order - Order array or function.
	 * @return string - Order subquery.
	 */
	public function buildOrder($order) {
		if (is_string($order)) {
			return $order;
		}
		$query = "";
		foreach ($order as $field => $order) {
			if (strlen($query)) {
				$query.= ", ";
			}
			$query.= $field . " " . $order;
		}
		return $query;
	}

	/**
	 * Update one or more rows that match the fields given by the selector array,
	 * and set their data as given by the values array.
	 *
	 * @param string $table - Table to update.
	 * @param array $values - New values to update.
	 * @param array $selector - Rows affected will be the ones that match these values. (Default: null - Matches everything).
	 * @param int $limit - Limit the number of rows affected. (Default: 0 - Unlimited).
	 * @param int $flags - Query construction flags. (Default: SELECTOR_AND | SELECTOR_EQUAL).
	 * @return int - The number of affected rows, or null on failure.
	 */
	public function updateRows($table, $values, $selector = null, $limit = 0, $flags = 0) {
		$table = $this->getTableName($table);
		$query = "UPDATE ";
		if ($limit > 0) {
			$query.= " TOP (" . intval($limit) . ") ";
		}
		$query.= $this->tickString($table);
		$query.= " SET "   . $this->buildSelector($values, MSSQL::SELECTOR_COMMA);
		if (is_array($selector) && sizeof($selector) > 0) {
			$query.= " WHERE " . $this->buildSelector($selector, $flags);
		}
		if ($h = $this->executeQuery($query))
		{
			if ($this->function == "MSSQL")
			{
				sqlsrv_rows_affected($h);
			}
			else
			{
				return $h->rowCount();
			}
		} else return null;
	}

	/**
	 * Delete one or more rows that match the fields given by the selector array.
	 *
	 * @param string $table - Table to update.
	 * @param array $selector - Rows affected will be the ones that match these values. (Default: null - Matches everything).
	 * @param int $limit - Limit the number of rows affected. (Default: 0).
	 * @param int $flags - Query construction flags. (Default: SELECTOR_AND | SELECTOR_EQUAL).
	 * @return int - The number of affected rows, or null on failure.
	 */
	public function deleteRows($table, $selector = null, $limit = 0, $flags = 0) {
		$table = $this->getTableName($table);
		$query = "DELETE FROM " . $this->tickString($table);
		if (is_array($selector) && sizeof($selector) > 0) {
			$query.= " WHERE " . $this->buildSelector($selector, $flags);
		}
		if ($limit > 0) {
			$query.= " LIMIT " . $limit;
		}
		if ($h = $this->executeQuery($query)) {
			if ($this->function == "MSSQL") {
				mssql_rows_affected($this->connection);
			} else {
				return $h->rowCount();
			}
		} else return null;
	}

	/**
	 * Insert a new row into the database.
	 *
	 * @param string $table - Table where to insert.
	 * @param array $values - An associative array with the fields and values to insert.
	 * @return int - The id of the newly inserted row (AUTO_INCREMENT field), or 0 if there is no associated ID, or null on failure.
	 */
	public function insertRow($table, $values) {
		$table = $this->getTableName($table);
		if (!is_array($values) || sizeof($values) < 1) {
			return null;
		}

		foreach ($values as $field => $value) {
			$q_fields[] = $this->tickString($field);
			$q_values[] = $this->quoteString($value);
		}
		$query = "INSERT INTO " . $this->tickString($table) . " (";
		$query.= implode(", ", $q_fields);
		$query .= ") VALUES (";
		$query.= implode(", ", $q_values);
		$query .= ")";
		$this->executeQuery($query);
	}

	/**
	 * Returns an array with a list of the tables available in this database.
	 *
	 * @return array - List of tables.
	 */
	public function listTables() {
		$tables = array();
		$result = $this->executeQuery('SHOW TABLES FROM ' . $this->tickString($this->database));
		while ($row = mssql_fetch_array($result)) {
			$tables[] = $row[0];
		}
		return $tables;
	}
}

?>
