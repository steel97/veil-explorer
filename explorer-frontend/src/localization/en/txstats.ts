export default () => {
    return {
        "Title": "Transactions statistics",
        "Meta": {
            "Title": "Transactions statistics",
            "Description": "View veil transactions statistics"
        },
        "Charts": {
            "day": {
                "Counts": {
                    "Title": "Transactions, 24hr",
                    "XAxis": "Block",
                    "YAxis": "Tx Count"
                },
                "Rates": {
                    "Title": "Tx Rate, 24hr",
                    "XAxis": "Block",
                    "YAxis": "Tx / Sec"
                }
            },
            "week": {
                "Counts": {
                    "Title": "Transactions, week",
                    "XAxis": "Block",
                    "YAxis": "Tx Count"
                },
                "Rates": {
                    "Title": "Tx Rate, week",
                    "XAxis": "Block",
                    "YAxis": "Tx / Sec"
                }
            },
            "month": {
                "Counts": {
                    "Title": "Transactions, month",
                    "XAxis": "Block",
                    "YAxis": "Tx Count"
                },
                "Rates": {
                    "Title": "Tx Rate, month",
                    "XAxis": "Block",
                    "YAxis": "Tx / Sec"
                }
            },
            "overall": {
                "Counts": {
                    "Title": "Transactions, overall",
                    "XAxis": "Block",
                    "YAxis": "Tx Count"
                },
                "Rates": {
                    "Title": "Tx Rate, overall",
                    "XAxis": "Block",
                    "YAxis": "Tx / Sec"
                }
            }
        }
    }
}