class Functions {
    static except(selected, remove) {
        let copy = Object.assign({}, selected);
        delete copy[remove];
        return copy;
    }

    static union(selected, add) {
        let copy = Object.assign({}, selected);
        copy[add] = true;
        return copy;
    }

    static toUtcDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}`;
        const timeStr = `${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())}:00.000Z`;
        return `${dateStr}T${timeStr}`;
    }

    static toLocalDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
        const timeStr = `${pad(date.getHours())}:${pad(date.getMinutes())}`;
        return `${dateStr}T${timeStr}`;
    }

    static playerSortFunction(playerA, playerB) {
        if (playerA.name.toLowerCase() === playerB.name.toLowerCase()) {
            return 0;
        }

        return (playerA.name.toLowerCase() > playerB.name.toLowerCase())
            ? 1
            : -1;
    }

    static gameSortFunction(a, b) {
        const aDate = new Date(a.date);
        const bDate = new Date(b.date);

        return bDate.getTime() - aDate.getTime();
    }
}

export { Functions };