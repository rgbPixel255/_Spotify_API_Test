const express = require('express');
const app = express();
const port = 3000;
const sqlite3 = require('sqlite3').verbose();
let db = new sqlite3.Database('./spotify-api-test.sqlite');
const path = require('path');

app.get('/callback', (req, res) => {
    const authorizationCode = req.query.code;
    db.run('DELETE FROM AuthorizationCodes', function(err) {
        if(err) {
            return console.log(err.message);
        }
        console.log(`All old codes were deleted`);
    });
    db.run('INSERT INTO AuthorizationCodes(code) VALUES(?)', [authorizationCode],
        function(err) {
            if(werr) {
                return console.log(err.message);
            }
            console.log(`A row was inserted`);
        });
    res.sendFile(path.join(__dirname, 'public/index.html'));
});

app.listen(port, () => {
    console.log(`Server running on http://localhost:${port}`);
});

app.use(express.static('public'));
