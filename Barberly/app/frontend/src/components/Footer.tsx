import { type FC } from "react";
import { BsCCircle } from "react-icons/bs";
import { FaFacebookSquare, FaLinkedin } from "react-icons/fa";
import { FaSquareInstagram, FaSquareXTwitter } from "react-icons/fa6";
import { NavLink, useNavigate } from "react-router-dom";

const Footer: FC = () => {
  const navigate = useNavigate();

  const mainLinks = [
    { name: "Home", to: "/" },
    { name: "Barbers", to: "/barbers" },
    { name: "About Us", to: "about-us", isSection: true },
  ];

  const serviceLinks = [
    { name: "Haircut", to: "#" },
    { name: "Beard Trim", to: "#" },
    { name: "Hair Styling", to: "#" },
  ];

  const handleScroll = (id: string): void => {
    navigate(`/#${id}`);
  };

  return (
    <footer className="w-full h-fit p-5 pt-12 sm:pt-16 bg-neutral-100 dark:bg-custom-gray border-t border-black/5 dark:border-white/5 overflow-hidden transition-colors duration-300">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-8 max-w-7xl mx-auto px-4 sm:px-10 text-center sm:text-left">
        <div className="flex flex-col items-center sm:items-start min-w-50">
          <h1 className="font-extrabold text-2xl tracking-tight">Contact us</h1>
          <div className="pt-4 space-y-2 text-sm sm:text-base text-black/70 dark:text-white/70">
            <p>
              <span className="font-semibold text-black dark:text-white">
                Customer Support:
              </span>
              <br />9 AM - 6 PM (Mon- Fri)
            </p>
            <p
              className="cursor-pointer hover:text-blue-500 transition-colors"
              onClick={() => {
                window.location.href = "tel:+1234567890";
              }}
            >
              <span className="font-semibold text-black dark:text-white">
                Call us:
              </span>{" "}
              +1 234 567 890
            </p>
            <p
              className="cursor-pointer hover:text-red-500 transition-colors"
              onClick={() => {
                window.location.href =
                  "mailto:barberly@support.com?subject=Support Request";
              }}
            >
              <span className="font-semibold text-black dark:text-white">
                Mail:
              </span>{" "}
              barberly@support.com
            </p>
          </div>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-37.5">
          <h1 className="font-extrabold text-2xl tracking-tight">Services</h1>
          <ul className="flex gap-3 flex-col pt-4 text-sm sm:text-base list-none">
            {serviceLinks.map(({ name }, index) => (
              <li key={index}>
                <p className="text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors">
                  {name}
                </p>
              </li>
            ))}
          </ul>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-37.5">
          <h1 className="font-extrabold text-2xl tracking-tight">Barberly</h1>
          <ul className="flex gap-3 flex-col pt-4 text-sm sm:text-base list-none">
            {mainLinks.map(({ name, to, isSection }, index) => (
              <li key={index}>
                {isSection ? (
                  <span
                    onClick={() => handleScroll(to)}
                    className="text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors cursor-pointer"
                  >
                    {name}
                  </span>
                ) : (
                  <NavLink
                    to={to}
                    className={({ isActive }) =>
                      isActive
                        ? "text-blue-600 dark:text-blue-400 font-semibold"
                        : "text-black/50 hover:text-black dark:text-white/50 hover:dark:text-white transition-colors"
                    }
                  >
                    {name}
                  </NavLink>
                )}
              </li>
            ))}
          </ul>
        </div>

        <div className="flex flex-col items-center sm:items-start min-w-55 lg:items-end lg:text-right">
          <h1 className="font-extrabold text-2xl tracking-tight">Stay Fresh</h1>
          <p className="pt-4 text-sm text-black/60 dark:text-white/60 mb-3 max-w-62.5">
            Subscribe to get special offers and barber tips.
          </p>
          <div className="flex w-full max-w-65 rounded-xl overflow-hidden border border-black/10 dark:border-white/10 p-0.5 bg-slate-100 dark:bg-white/5 backdrop-blur-md">
            <input
              type="email"
              placeholder="Your email..."
              className="w-full bg-transparent px-3 text-sm focus:outline-hidden text-black dark:text-white placeholder-black/40 dark:placeholder-white/40"
            />
            <button className="bg-linear-to-r from-blue-600 to-red-500 dark:from-blue-500 dark:to-red-500 text-white px-4 py-1.5 rounded-lg text-xs font-bold hover:opacity-90 transition-opacity cursor-pointer">
              Join
            </button>
          </div>

          <div className="flex gap-4 items-center justify-center sm:justify-start lg:justify-end w-full h-fit mt-6">
            <FaSquareInstagram
              className="h-6 w-6 cursor-pointer text-black/60 dark:text-white/60 hover:text-black hover:dark:text-white transition-colors"
              onClick={() => window.open("https://www.instagram.com/")}
            />
            <FaFacebookSquare
              className="h-6 w-6 cursor-pointer text-black/60 dark:text-white/60 hover:text-black hover:dark:text-white transition-colors"
              onClick={() => window.open("https://www.facebook.com/")}
            />
            <FaSquareXTwitter
              className="h-6 w-6 cursor-pointer text-black/60 dark:text-white/60 hover:text-black hover:dark:text-white transition-colors"
              onClick={() => window.open("https://www.twitter.com/")}
            />
            <FaLinkedin
              className="h-6 w-6 cursor-pointer text-black/60 dark:text-white/60 hover:text-black hover:dark:text-white transition-colors"
              onClick={() => window.open("https://www.linkedin.com/")}
            />
          </div>
        </div>
      </div>

      <div className="w-full text-center mt-12 sm:mt-16 text-xs sm:text-sm text-black/40 dark:text-white/40">
        <hr className="border-black/5 dark:border-white/5 mb-4" />
        <p className="flex justify-center items-center gap-2 py-2">
          Barberly
          <BsCCircle className="text-[10px] sm:text-xs" />
          by DAMM. All right reserved. {new Date().getFullYear()}.
        </p>
      </div>
    </footer>
  );
};

export default Footer;
