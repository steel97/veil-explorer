export default () => {
    return {
        "Title": "Статистика транзакций",
        "Meta": {
            "Title": "Статистика транзакций",
            "Description": "Просмотр статистики транзакций"
        },
        "Charts": {
            "day": {
                "Counts": {
                    "Title": "Транзакции, 24hr",
                    "XAxis": "Блок",
                    "YAxis": "Tx Кол-во"
                },
                "Rates": {
                    "Title": "Tx Скорость, 24ч",
                    "XAxis": "Блок",
                    "YAxis": "Tx / Сек"
                }
            },
            "week": {
                "Counts": {
                    "Title": "Транзакции, неделя",
                    "XAxis": "Блок",
                    "YAxis": "Tx Кол-во"
                },
                "Rates": {
                    "Title": "Tx Скорость, неделя",
                    "XAxis": "Блок",
                    "YAxis": "Tx / Сек"
                }
            },
            "month": {
                "Counts": {
                    "Title": "Транзакции, месяц",
                    "XAxis": "Блок",
                    "YAxis": "Tx Кол-во"
                },
                "Rates": {
                    "Title": "Tx Скорость, месяц",
                    "XAxis": "Блок",
                    "YAxis": "Tx / Сек"
                }
            },
            "overall": {
                "Counts": {
                    "Title": "Транзакции, общие",
                    "XAxis": "Блок",
                    "YAxis": "Tx Кол-во"
                },
                "Rates": {
                    "Title": "Tx Скорость, общая",
                    "XAxis": "Блок",
                    "YAxis": "Tx / Сек"
                }
            }
        }
    }
}