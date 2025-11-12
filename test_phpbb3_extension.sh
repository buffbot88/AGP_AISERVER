#!/bin/bash

echo "=========================================="
echo "phpBB3 Extension Structure Validation"
echo "=========================================="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counters
TESTS_PASSED=0
TESTS_FAILED=0

# Helper function for test results
test_result() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
        ((TESTS_PASSED++))
    else
        echo -e "${RED}✗${NC} $2"
        ((TESTS_FAILED++))
    fi
}

BASE_DIR="/home/runner/work/ASHATOS/ASHATOS/phpbb3_extension/ashatos/authbridge"

echo "1. Checking extension directory structure..."

# Check main directory
[ -d "$BASE_DIR" ]
test_result $? "Extension directory exists"

# Check required files
[ -f "$BASE_DIR/composer.json" ]
test_result $? "composer.json exists"

[ -f "$BASE_DIR/ext.php" ]
test_result $? "ext.php exists"

[ -f "$BASE_DIR/README.md" ]
test_result $? "README.md exists"

echo ""
echo "2. Checking configuration files..."

# Check config directory
[ -d "$BASE_DIR/config" ]
test_result $? "config directory exists"

[ -f "$BASE_DIR/config/services.yml" ]
test_result $? "services.yml exists"

[ -f "$BASE_DIR/config/routing.yml" ]
test_result $? "routing.yml exists"

echo ""
echo "3. Checking controller files..."

# Check controller directory
[ -d "$BASE_DIR/controller" ]
test_result $? "controller directory exists"

[ -f "$BASE_DIR/controller/api_controller.php" ]
test_result $? "api_controller.php exists"

echo ""
echo "4. Checking event listener files..."

# Check event directory
[ -d "$BASE_DIR/event" ]
test_result $? "event directory exists"

[ -f "$BASE_DIR/event/listener.php" ]
test_result $? "listener.php exists"

echo ""
echo "5. Checking language files..."

# Check language directory
[ -d "$BASE_DIR/language/en" ]
test_result $? "language/en directory exists"

[ -f "$BASE_DIR/language/en/common.php" ]
test_result $? "common.php exists"

echo ""
echo "6. Validating PHP syntax..."

# Check PHP syntax for all PHP files
for php_file in $(find "$BASE_DIR" -name "*.php"); do
    php -l "$php_file" > /dev/null 2>&1
    test_result $? "PHP syntax valid: $(basename $php_file)"
done

echo ""
echo "7. Checking JSON syntax..."

# Check JSON syntax
php -r "json_decode(file_get_contents('$BASE_DIR/composer.json')); if(json_last_error() != JSON_ERROR_NONE) exit(1);"
test_result $? "composer.json is valid JSON"

echo ""
echo "8. Checking YAML syntax..."

# Basic YAML check (verify files are not empty and have proper structure)
if [ -s "$BASE_DIR/config/services.yml" ] && grep -q "services:" "$BASE_DIR/config/services.yml"; then
    test_result 0 "services.yml has valid structure"
else
    test_result 1 "services.yml has valid structure"
fi

if [ -s "$BASE_DIR/config/routing.yml" ] && grep -q "path:" "$BASE_DIR/config/routing.yml"; then
    test_result 0 "routing.yml has valid structure"
else
    test_result 1 "routing.yml has valid structure"
fi

echo ""
echo "9. Checking API endpoints in routing..."

# Check for required endpoints
grep -q "api/auth/login" "$BASE_DIR/config/routing.yml"
test_result $? "Login endpoint defined in routing"

grep -q "api/auth/register" "$BASE_DIR/config/routing.yml"
test_result $? "Register endpoint defined in routing"

grep -q "api/auth/validate" "$BASE_DIR/config/routing.yml"
test_result $? "Validate endpoint defined in routing"

grep -q "api/auth/logout" "$BASE_DIR/config/routing.yml"
test_result $? "Logout endpoint defined in routing"

echo ""
echo "10. Checking API controller methods..."

# Check for required methods in controller
grep -q "public function login()" "$BASE_DIR/controller/api_controller.php"
test_result $? "login() method exists"

grep -q "public function register()" "$BASE_DIR/controller/api_controller.php"
test_result $? "register() method exists"

grep -q "public function validate()" "$BASE_DIR/controller/api_controller.php"
test_result $? "validate() method exists"

grep -q "public function logout()" "$BASE_DIR/controller/api_controller.php"
test_result $? "logout() method exists"

echo ""
echo "=========================================="
echo "Test Summary"
echo "=========================================="
echo -e "${GREEN}Passed:${NC} $TESTS_PASSED"
echo -e "${RED}Failed:${NC} $TESTS_FAILED"
echo ""

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}All tests passed! Extension structure is valid.${NC}"
    exit 0
else
    echo -e "${RED}Some tests failed. Please review the extension structure.${NC}"
    exit 1
fi
