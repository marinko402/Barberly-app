import { Outlet, useLocation } from "react-router-dom";
import { AnimatePresence } from "framer-motion";
import ScrollToTop from "../components/ScrollToTop";
import { AuthProvider } from "../context/auth/AuthProvider";
import Navbar from "../components/Navbar";
import Footer from "../components/Footer";

const Layout = () => {
  const { pathname } = useLocation();

  const hideLayout =
    pathname.startsWith("/login") || pathname.startsWith("/register");

  return (
    <>
      <AuthProvider>
        {!hideLayout && <Navbar />}
        <AnimatePresence mode="wait">
          <main className="w-full h-full overflow-hidden dark:bg-custom-gray dark:text-white">
            <ScrollToTop />
            <Outlet />
          </main>
        </AnimatePresence>
        {!hideLayout && <Footer />}
      </AuthProvider>
    </>
  );
};

export default Layout;
