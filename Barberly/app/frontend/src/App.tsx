import { RouterProvider } from "react-router";
import { router } from "./routes/router";
import { useState } from "react";
import { ThemeProvider } from "./context/theme/theme-provider";
import { ReactQueryDevtoolsPanel } from "@tanstack/react-query-devtools";

const App = () => {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <>
      <ThemeProvider defaultTheme="light" storageKey="theme-mode">
        <RouterProvider router={router} />
        <button
          onClick={() => setIsOpen(!isOpen)}
        >{`${isOpen ? "Close" : "Open"} the devtools panel`}</button>
        {isOpen && <ReactQueryDevtoolsPanel onClose={() => setIsOpen(false)} />}
      </ThemeProvider>
    </>
  );
};

export default App;
