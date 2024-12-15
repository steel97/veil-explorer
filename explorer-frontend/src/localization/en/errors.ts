export default () => {
  return {
    ToHome: "Back to homepage",
    Error404: {
      Title: "Error 404 - Page not found",
      Description: "Requested page cannot be found on server",
      Meta: {
        Title: "Veil Explorer - 404 Page Not Found",
        Description: "Requested page was not found on server",
      },
    },
    Error500: {
      Title: "Error 500 - Internal server error",
      Description: "Server cannot process request",
      Meta: {
        Title: "Veil Explorer - Internal server error",
        Description: "Internal server error, cannot process user request",
      },
    },
  };
};