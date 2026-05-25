import { NavLink, useNavigate } from "react-router-dom";
import { IoMenu } from "react-icons/io5";
import { useState, type FC } from "react";
import { CgProfile } from "react-icons/cg";
import { IoClose } from "react-icons/io5";
import { useTheme } from "../context/theme/use-theme";
import { useAuth } from "../context/auth/useAuth";

type NavLinkType = {
  name: string;
  to: string;
  isSection?: boolean;
};

const Navbar: FC = () => {
  const navigate = useNavigate();
  const { theme, setTheme } = useTheme();
  const { isLoggedIn } = useAuth();
  const toNavigate = isLoggedIn() ? "profile" : "login";

  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const isDarkMode = theme === "dark";

  const handleToggle = () => {
    setTheme(isDarkMode ? "light" : "dark");
  };

  const toggleMenu = () => setIsMenuOpen((prev) => !prev);

  const navLinks: NavLinkType[] = [
    {
      name: "Home",
      to: "/",
    },
    {
      name: "About Us",
      to: "about-us",
      isSection: true,
    },
    {
      name: "Barbers",
      to: "/barbers",
    },
  ];

  const handleScroll = (id: string): void => {
    navigate(`/#${id}`);
  };

  return (
    <>
      <header className="fixed z-100 text-white w-full h-23 flex">
        <nav className="w-full flex justify-between items-center bg-white bg-[linear-gradient(67deg,rgba(255,255,255,0.7)_0%,rgba(255,255,255,0.7)_20%,rgba(59,130,246,0.7)_20%,rgba(59,130,246,0.7)_40%,rgba(255,255,255,0.7)_40%,rgba(255,255,255,0.7)_60%,rgba(239,68,68,0.7)_60%,rgba(239,68,68,0.7)_80%,rgba(255,255,255,0.7)_80%,rgba(255,255,255,0.7)_100%)] bg-size-[100px_60px] animate-barber border border-black dark:border-white rounded-4xl m-5">
          <NavLink
            to="/"
            className="ml-3 w-32 h-8 bg-cover bg-no-repeat bg-[url('assets/images/barberly.png')]"
          ></NavLink>

          <div className="max-lg:hidden justify-self-center">
            <ul className="flex gap-5">
              {navLinks.map(({ name, to, isSection }) => (
                <li className="list-none" key={to}>
                  {isSection ? (
                    <span
                      onClick={() => handleScroll(to)}
                      className="cursor-pointer text-black text-opacity-50 dark:text-opacity-50 "
                    >
                      {name}
                    </span>
                  ) : (
                    <NavLink
                      to={to}
                      className={({ isActive }) =>
                        isActive
                          ? "text-black text-opacity-100 font-bold"
                          : "text-black text-opacity-50 d hover:text-opacity-100 "
                      }
                    >
                      {name}
                    </NavLink>
                  )}
                </li>
              ))}
            </ul>
          </div>

          <div className="flex gap-2 max-lg:justify-self-ends ">
            {isLoggedIn() ? (
              <NavLink
                className="bg-white/10 rounded-2xl backdrop-blur-md border border-white/20 hover:bg-white/20 p-1 justify-center items-center cursor-pointer text-black"
                to={toNavigate}
              >
                <CgProfile className="w-8 h-8" />
              </NavLink>
            ) : (
              <NavLink
                className="flex  bg-white/10 rounded-2xl backdrop-blur-md border border-white/20 hover:bg-white/20 p-1 px-4 gap-2 justify-center items-center cursor-pointer text-black "
                to={toNavigate}
              >
                <CgProfile className="w-6 h-6" />
                <span>Login</span>
              </NavLink>
            )}

            <div className="max-lg:hidden mr-3 flex items-center justify-center">
              <label className="swap swap-rotate">
                <input
                  type="checkbox"
                  onChange={handleToggle}
                  checked={isDarkMode}
                />

                <svg
                  className="swap-off h-10 w-10 fill-current bg-white/10 rounded-2xl backdrop-blur-md border border-white/20 hover:bg-white/20 p-1 justify-center items-center cursor-pointer text-black"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 24 24"
                >
                  <path d="M5.64,17l-.71.71a1,1,0,0,0,0,1.41,1,1,0,0,0,1.41,0l.71-.71A1,1,0,0,0,5.64,17ZM5,12a1,1,0,0,0-1-1H3a1,1,0,0,0,0,2H4A1,1,0,0,0,5,12Zm7-7a1,1,0,0,0,1-1V3a1,1,0,0,0-2,0V4A1,1,0,0,0,12,5ZM5.64,7.05a1,1,0,0,0,.7.29,1,1,0,0,0,.71-.29,1,1,0,0,0,0-1.41l-.71-.71A1,1,0,0,0,4.93,6.34Zm12,.29a1,1,0,0,0,.7-.29l.71-.71a1,1,0,1,0-1.41-1.41L17,5.64a1,1,0,0,0,0,1.41A1,1,0,0,0,17.66,7.34ZM21,11H20a1,1,0,0,0,0,2h1a1,1,0,0,0,0-2Zm-9,8a1,1,0,0,0-1,1v1a1,1,0,0,0,2,0V20A1,1,0,0,0,12,19ZM18.36,17A1,1,0,0,0,17,18.36l.71.71a1,1,0,0,0,1.41,0,1,1,0,0,0,0-1.41ZM12,6.5A5.5,5.5,0,1,0,17.5,12,5.51,5.51,0,0,0,12,6.5Zm0,9A3.5,3.5,0,1,1,15.5,12,3.5,3.5,0,0,1,12,15.5Z" />
                </svg>

                <svg
                  className="swap-on h-10 w-10 fill-current bg-white/10 rounded-2xl backdrop-blur-md border border-white/20 hover:bg-white/20 p-1 justify-center items-center cursor-pointer text-black"
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 24 24"
                >
                  <path d="M21.64,13a1,1,0,0,0-1.05-.14,8.05,8.05,0,0,1-3.37.73A8.15,8.15,0,0,1,9.08,5.49a8.59,8.59,0,0,1,.25-2A1,1,0,0,0,8,2.36,10.14,10.14,0,1,0,22,14.05,1,1,0,0,0,21.64,13Zm-9.5,6.69A8.14,8.14,0,0,1,7.08,5.22v.27A10.15,10.15,0,0,0,17.22,15.63a9.79,9.79,0,0,0,2.1-.22A8.11,8.11,0,0,1,12.14,19.73Z" />
                </svg>
              </label>
            </div>

            <IoMenu
              className="lg:hidden h-10 w-10 mr-3 bg-white/10 rounded-2xl backdrop-blur-md border border-white/20 hover:bg-white/20 p-1 justify-center items-center cursor-pointer text-black"
              onClick={() => {
                toggleMenu();
              }}
            />
          </div>

          {isMenuOpen && (
            <>
              <div
                onClick={toggleMenu}
                className={`fixed inset-0 bg-black/40 backdrop-blur-sm z-40 transition-opacity duration-300 ${
                  isMenuOpen
                    ? "opacity-100 pointer-events-auto"
                    : "opacity-0 pointer-events-none"
                }`}
              />
              <div
                className={`flex flex-col items-center justify-center gap-5 z-50 fixed right-0 top-0 h-screen w-60 bg-black/50 backdrop-blur-lg border-l border-white/10 transition-all duration-300 ease-out will-change-transform ${
                  isMenuOpen
                    ? "translate-x-0 opacity-100"
                    : "translate-x-full opacity-0"
                }`}
              >
                <IoClose
                  className="cursor-pointer h-8 w-8 absolute top-5 right-5"
                  onClick={toggleMenu}
                />
                <ul className="text-white flex flex-col gap-5 text-center text-2xl">
                  {navLinks.map(({ name, to, isSection }) => (
                    <li
                      className="list-none"
                      key={to}
                      onClick={() => {
                        toggleMenu();
                      }}
                    >
                      {isSection ? (
                        <span
                          onClick={() => handleScroll(to)}
                          className="cursor-pointer  text-opacity-50 dark:text-opacity-50"
                        >
                          {name}
                        </span>
                      ) : (
                        <NavLink
                          to={to}
                          className={({ isActive }) =>
                            isActive
                              ? ""
                              : " text-opacity-50 dark:text-opacity-50 hover:text-opacity-100"
                          }
                        >
                          {name}
                        </NavLink>
                      )}
                    </li>
                  ))}
                </ul>
                <div className="ml-3 flex items-center justify-center">
                  <label className="swap swap-rotate">
                    <input
                      type="checkbox"
                      onChange={handleToggle}
                      checked={isDarkMode}
                    />

                    <svg
                      className="swap-off h-10 w-10 fill-current "
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 24 24"
                    >
                      <path d="M5.64,17l-.71.71a1,1,0,0,0,0,1.41,1,1,0,0,0,1.41,0l.71-.71A1,1,0,0,0,5.64,17ZM5,12a1,1,0,0,0-1-1H3a1,1,0,0,0,0,2H4A1,1,0,0,0,5,12Zm7-7a1,1,0,0,0,1-1V3a1,1,0,0,0-2,0V4A1,1,0,0,0,12,5ZM5.64,7.05a1,1,0,0,0,.7.29,1,1,0,0,0,.71-.29,1,1,0,0,0,0-1.41l-.71-.71A1,1,0,0,0,4.93,6.34Zm12,.29a1,1,0,0,0,.7-.29l.71-.71a1,1,0,1,0-1.41-1.41L17,5.64a1,1,0,0,0,0,1.41A1,1,0,0,0,17.66,7.34ZM21,11H20a1,1,0,0,0,0,2h1a1,1,0,0,0,0-2Zm-9,8a1,1,0,0,0-1,1v1a1,1,0,0,0,2,0V20A1,1,0,0,0,12,19ZM18.36,17A1,1,0,0,0,17,18.36l.71.71a1,1,0,0,0,1.41,0,1,1,0,0,0,0-1.41ZM12,6.5A5.5,5.5,0,1,0,17.5,12,5.51,5.51,0,0,0,12,6.5Zm0,9A3.5,3.5,0,1,1,15.5,12,3.5,3.5,0,0,1,12,15.5Z" />
                    </svg>

                    <svg
                      className="swap-on h-10 w-10 fill-current "
                      xmlns="http://www.w3.org/2000/svg"
                      viewBox="0 0 24 24"
                    >
                      <path d="M21.64,13a1,1,0,0,0-1.05-.14,8.05,8.05,0,0,1-3.37.73A8.15,8.15,0,0,1,9.08,5.49a8.59,8.59,0,0,1,.25-2A1,1,0,0,0,8,2.36,10.14,10.14,0,1,0,22,14.05,1,1,0,0,0,21.64,13Zm-9.5,6.69A8.14,8.14,0,0,1,7.08,5.22v.27A10.15,10.15,0,0,0,17.22,15.63a9.79,9.79,0,0,0,2.1-.22A8.11,8.11,0,0,1,12.14,19.73Z" />
                    </svg>
                  </label>
                </div>
              </div>
            </>
          )}
        </nav>
      </header>
    </>
  );
};

export default Navbar;
