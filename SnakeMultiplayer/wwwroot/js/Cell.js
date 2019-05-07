var MoveDirection = Object.freeze({
    "None": 0,
    "Up": 1,
    "Right": 2,
    "Down": 3,
    "Left": 4
});

class Cell {
    constructor(length = -1, innerColor, outlineColor, x = -1, y = -1) {
        this.size = length,
            this.innerColor = innerColor,
            this.outlineColor = outlineColor
        this.x = x;
        this.y = y;
    }
    update(direction) {
        switch (direction) {
            case MoveDirection.Up:
                this.y -= 1;
                break;
            case MoveDirection.Right:
                this.x += 1;
                break;
            case MoveDirection.Down:
                this.y += 1;
                break;
            case MoveDirection.Left:
                this.x -= 1;
                break;
            case MoveDirection.None:
                break;
            default:
                console.error("Unexpected direction value!", direction);
                return;
        }
    }
    getCopy() {
        return new Cell(this.length, this.innerColor, this.outlineColor, this.x, this.y);
    }
}