class CustomLinkedListNode {
    constructor(element) {
        this.element = element;
        this.next = null;
        this.previous = null;
    }
}

class CustomLinkedList {
    constructor() {
        this.count = 0;
        this.head = null;
        this.tail = null;
    }

    getFirst() {
        return this.head.element.getCopy();
    }

    addFirst(element) {
        var newNode = new CustomLinkedListNode(element);

        if (this.head == null) {
            this.head = newNode;
        } else if (this.count === 1) {
            newNode.next = this.head;
            this.head = newNode;
            this.tail = this.head.next;
            this.tail.previous = this.head;
        } else {
            newNode.next = this.head
            this.head = newNode;
            this.head.next.previous = this.head;
        }
        this.count += 1;
    }

    deleteLast() {
        var deleted;
        this.count -= 1;

        if (this.head === null) {
            console.warn("Trying to delete element from empty linked list!");
            return null;
        } else if (this.head.next == null) {
            deleted = this.head.element;
            this.head = null;
            return deleted;
        } else {
            deleted = this.tail.element;
            this.tail = this.tail.previous;
            this.tail.next = null;
            return deleted;
        }
    }

    getArray() {

        var array = [];
        var currNode = this.head;

        while (currNode !== null) {
            //array.push(currNode.element.getCopy());
            array.push(currNode.element);
            currNode = currNode.next;
        }
        return array.slice();
    }

    print() {
        console.log("LinkedList elements:");
        var node = this.head;
        while (node !== null) {
            console.log(node.element);
            node = node.next;
        }
    }
}