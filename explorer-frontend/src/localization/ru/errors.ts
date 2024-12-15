export default () => {
  return {
    ToHome: "Вернуться на главную",
    Error404: {
      Title: "Ошибка 404 - Страница не найдена",
      Description: "Запрашиваемая страница не найдена",
      Meta: {
        Title: "404 Страница не найдена",
        Description: "Запрашиваемая страница не найдена",
      },
    },
    Error500: {
      Title: "Ошибка 500 - Внутренняя ошибка",
      Description: "Сервер не может обработать запрос",
      Meta: {
        Title: "Внутренняя ошибка",
        Description: "Сервер не может обработать запрос",
      },
    },
  };
};